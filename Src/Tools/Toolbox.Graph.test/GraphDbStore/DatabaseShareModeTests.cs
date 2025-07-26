//using System.Collections.Concurrent;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Graph.test.Application;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Graph.test.GraphDbStore;

//public class DatabaseShareModeTests
//{
//    private readonly ITestOutputHelper _outputHelper;
//    public DatabaseShareModeTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

//    [Fact]
//    public async Task OneWriteOtherRead()
//    {
//        using var testClient = await TestApplication.CreateTestGraphService(logOutput: _outputHelper, shareMode: true, logPrefix: "one");
//        var context1 = testClient.CreateScopeContext<DatabaseShareModeTests>();
//        var context2 = testClient.CreateScopeContext<DatabaseShareModeTests>();
//        var fileStore = testClient.Services.GetRequiredService<IFileStore>();
//        var graphFileStore = testClient.Services.GetRequiredService<IGraphFileStore>();
//        var mapCounter = testClient.Services.GetRequiredService<GraphMapCounter>();
//        var leaseCounter = mapCounter.Leases;

//        using var SecondEngineClient = await GraphTestStartup.CreateGraphService(
//            logOutput: x => _outputHelper.WriteLine("[two] " + x),
//            config: x => x
//                .AddSingleton<IFileStore>(fileStore)
//                .AddSingleton<IGraphFileStore>(graphFileStore)
//                .AddSingleton<GraphMapCounter>(mapCounter)
//                .AddLogging(config => config.AddFilter("Toolbox.Graph.GraphQueryExecute", LogLevel.Warning)),
//            sharedMode: true,
//            useInMemoryStore: false
//            );

//        var e1 = await testClient.Execute("add node key=node1 set t1=v1, t2=v ;", context1);
//        e1.BeOk();
//        leaseCounter.Acquire.Value.Be(3);
//        leaseCounter.Release.Value.Be(3);
//        leaseCounter.ActiveAcquire.Value.Be(0);
//        leaseCounter.ActiveExclusive.Value.Be(0);

//        var q1 = await testClient.Execute("select (key=node1);", context1);
//        q1.Action(x =>
//        {
//            x.BeOk();
//            x.Return().Action(y =>
//            {
//                y.Nodes.Count.Be(1);
//                var node = y.Nodes[0].Key.Be("node1");
//            });
//        });

//        leaseCounter.Acquire.Value.Be(4);
//        leaseCounter.Release.Value.Be(4);
//        leaseCounter.ActiveAcquire.Value.Be(0);
//        leaseCounter.ActiveExclusive.Value.Be(0);

//        var s1 = await SecondEngineClient.Execute("select (key=node1);", context2);
//        s1.Action(x =>
//        {
//            x.BeOk();
//            x.Return().Action(y =>
//            {
//                y.Nodes.Count.Be(1);
//                var node = y.Nodes[0].Key.Be("node1");
//            });
//        });

//        leaseCounter.Acquire.Value.Be(5);
//        leaseCounter.Release.Value.Be(5);
//        leaseCounter.ActiveAcquire.Value.Be(0);
//        leaseCounter.ActiveExclusive.Value.Be(0);
//    }

//    [Fact]
//    public async Task ParallelReads()
//    {
//        using var testClient = await GraphTestStartup.CreateGraphService(logOutput: x => _outputHelper.WriteLine(x), sharedMode: true);
//        var context1 = testClient.CreateScopeContext<DatabaseShareModeTests>();
//        var context2 = testClient.CreateScopeContext<DatabaseShareModeTests>();
//        var fileStore = testClient.Services.GetRequiredService<IFileStore>();
//        var graphFileStore = testClient.Services.GetRequiredService<IGraphFileStore>();
//        var mapCounter = testClient.Services.GetRequiredService<GraphMapCounter>();
//        var leaseCounter = mapCounter.Leases;

//        using var secondEngineClient = await GraphTestStartup.CreateGraphService(
//            logOutput: x => _outputHelper.WriteLine(x),
//            config: x => x.AddSingleton<IFileStore>(fileStore)
//                .AddSingleton<IGraphFileStore>(graphFileStore)
//                .AddSingleton<GraphMapCounter>(mapCounter),
//            sharedMode: true,
//            useInMemoryStore: false
//            );

//        var e1 = await testClient.Execute("add node key=node1 set t1=v1, t2=v ;", context1);
//        e1.IsOk().BeTrue();
//        leaseCounter.Acquire.Value.Be(3);
//        leaseCounter.Release.Value.Be(3);
//        leaseCounter.ActiveExclusive.Value.Be(0);
//        leaseCounter.ActiveAcquire.Value.Be(0);

//        var q1 = await testClient.Execute("select (key=node1);", context1);
//        q1.Action(x =>
//        {
//            x.BeOk();
//            x.Return().Action(y =>
//            {
//                y.Nodes.Count.Be(1);
//                var node = y.Nodes[0].Key.Be("node1");
//            });
//        });

//        CancellationTokenSource tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
//        CancellationToken token = tokenSource.Token;
//        var list = new ConcurrentQueue<(int job, int index, DateTimeOffset date, int globalCount)>();
//        int globalCount = 0;

//        var t1 = Task.Run(async () =>
//        {
//            try
//            {
//                int count = 0;
//                while (!token.IsCancellationRequested)
//                {
//                    var v = Interlocked.Increment(ref globalCount);
//                    list.Enqueue((1, count++, DateTimeOffset.UtcNow, v));
//                    context1.LogInformation("T1 count={count} globalCount={v}", count, v);
//                    await query(testClient, context1);
//                }
//            }
//            catch (Exception ex)
//            {
//                context1.LogError(ex, "Error in T1 - canceling");
//                tokenSource.Cancel();
//                throw;
//            }
//        });

//        var t2 = Task.Run(async () =>
//        {
//            try
//            {
//                int count = 0;
//                while (!token.IsCancellationRequested)
//                {
//                    var v = Interlocked.Increment(ref globalCount);
//                    list.Enqueue((2, count++, DateTimeOffset.UtcNow, v));
//                    context1.LogInformation("T2 count={count} globalCount={v}", count, v);
//                    await query(secondEngineClient, context2);
//                }
//            }
//            catch (Exception ex)
//            {
//                context1.LogError(ex, "Error in T2 - canceling");
//                tokenSource.Cancel();
//                throw;
//            }
//        });

//        await Task.WhenAll(t1, t2);

//        static async Task<Option<QueryResult>> query(GraphHostService host, ScopeContext context)
//        {
//            var s1 = await host.Execute("select (key=node1);", context);
//            s1.Action(x =>
//            {
//                x.BeOk();
//                x.Return().Action(y =>
//                {
//                    y.Nodes.Count.Be(1);
//                    var node = y.Nodes[0].Key.Be("node1");
//                });
//            });

//            return s1;
//        }
//    }
//}
