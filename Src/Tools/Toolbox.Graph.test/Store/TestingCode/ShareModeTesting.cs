using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Store.TestingCode;

internal static class ShareModeTesting
{
    public static async Task OneWriteOtherRead(GraphHostService testClient, GraphHostService secondEngineClient, ScopeContext context)
    {
        var fileStore = testClient.Services.GetRequiredService<IFileStore>();
        var graphFileStore = testClient.Services.GetRequiredService<IGraphStore>();
        var mapCounter = testClient.Services.GetRequiredService<GraphMapCounter>();
        var leaseCounter = mapCounter.Leases;

        (await testClient.Execute("select (key=node1);", context)).Assert(x => x.IsOk() && x.Return().Nodes.Count == 0, "not 0");
        (await secondEngineClient.Execute("select (key=node1);", context)).Assert(x => x.IsOk() && x.Return().Nodes.Count == 0, "not 0");

        var e1 = await testClient.Execute("add node key=node1 set t1=v1, t2=v ;", context);
        e1.IsOk().BeTrue(e1.ToString());
        leaseCounter.Acquire.Value.Assert(x => x >= 1, "not >= 1");
        leaseCounter.Release.Value.Assert(x => x >= 1, "not >= 1");
        leaseCounter.ActiveAcquire.Value.Be(0);
        leaseCounter.ActiveExclusive.Value.Be(0);

        var q1 = await testClient.Execute("select (key=node1);", context);
        q1.Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().Action(y =>
            {
                y.Nodes.Count.Be(1);
                var node = y.Nodes[0].Key.Be("node1");
            });
        });

        leaseCounter.Acquire.Value.Assert(x => x >= 2, "not >= 2");
        leaseCounter.Release.Value.Assert(x => x >= 2, "not >= 2");
        leaseCounter.ActiveAcquire.Value.Be(0);
        leaseCounter.ActiveExclusive.Value.Be(0);

        var s1 = await secondEngineClient.Execute("select (key=node1);", context);
        s1.Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().Action(y =>
            {
                y.Nodes.Count.Be(1);
                var node = y.Nodes[0].Key.Be("node1");
            });
        });

        leaseCounter.Acquire.Value.Assert(x => x >= 3, "not >= 3");
        leaseCounter.Release.Value.Assert(x => x >= 3, "not >= 3");
        leaseCounter.ActiveAcquire.Value.Be(0);
        leaseCounter.ActiveExclusive.Value.Be(0);
    }

    public static async Task ParallelReads(GraphHostService testClient, GraphHostService secondEngineClient, ScopeContext context)
    {
        var fileStore = testClient.Services.GetRequiredService<IFileStore>();
        var graphFileStore = testClient.Services.GetRequiredService<IGraphStore>();
        var mapCounter = testClient.Services.GetRequiredService<GraphMapCounter>();
        var leaseCounter = mapCounter.Leases;

        (await testClient.Execute("select (key=node1);", context.WithNewTraceId())).Assert(x => x.IsOk() && x.Return().Nodes.Count == 0, "not 0");
        (await secondEngineClient.Execute("select (key=node1);", context.WithNewTraceId())).Assert(x => x.IsOk() && x.Return().Nodes.Count == 0, "not 0");

        var e1 = await testClient.Execute("add node key=node1 set t1=v1, t2=v ;", context.WithNewTraceId());
        e1.IsOk().BeTrue();
        leaseCounter.Acquire.Value.Assert(x => x >= 1, "not >= 1");
        leaseCounter.Release.Value.Assert(x => x >= 1, "not >= 1");
        leaseCounter.ActiveExclusive.Value.Be(0);
        leaseCounter.ActiveAcquire.Value.Be(0);

        var q1 = await testClient.Execute("select (key=node1);", context.WithNewTraceId());
        q1.Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().Action(y =>
            {
                y.Nodes.Count.Be(1);
                var node = y.Nodes[0].Key.Be("node1");
            });
        });

        CancellationTokenSource tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        CancellationToken token = tokenSource.Token;
        var list = new ConcurrentQueue<(int job, int index, DateTimeOffset date, int globalCount)>();
        int globalCount = 0;

        var t1 = Task.Run(async () =>
        {
            try
            {
                int count = 0;
                while (!token.IsCancellationRequested)
                {
                    var v = Interlocked.Increment(ref globalCount);
                    list.Enqueue((1, count++, DateTimeOffset.UtcNow, v));
                    context.LogInformation("T1 count={count} globalCount={v}", count, v);
                    await query(testClient, context);
                }
            }
            catch (Exception ex)
            {
                context.LogError(ex, "Error in T1 - canceling");
                tokenSource.Cancel();
                throw;
            }
        });

        var t2 = Task.Run(async () =>
        {
            try
            {
                int count = 0;
                while (!token.IsCancellationRequested)
                {
                    var v = Interlocked.Increment(ref globalCount);
                    list.Enqueue((2, count++, DateTimeOffset.UtcNow, v));
                    context.LogInformation("T2 count={count} globalCount={v}", count, v);
                    await query(secondEngineClient, context);
                }
            }
            catch (Exception ex)
            {
                context.LogError(ex, "Error in T2 - canceling");
                tokenSource.Cancel();
                throw;
            }
        });

        await Task.WhenAll(t1, t2);

        static async Task<Option<QueryResult>> query(GraphHostService host, ScopeContext context)
        {
            var s1 = await host.Execute("select (key=node1);", context.WithNewTraceId());
            s1.Action(x =>
            {
                x.BeOk();
                x.Return().Action(y =>
                {
                    y.Nodes.Count.Be(1);
                    var node = y.Nodes[0].Key.Be("node1");
                });
            });

            return s1;
        }
    }
}
