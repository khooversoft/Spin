using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Azure;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Transactions;

public class GraphJournalStressTest
{
    private const string _dbCopyKeyedName = "db-copy";

    private readonly ITestOutputHelper _logOutput;
    private record Payload(string Name, int Count);
    public GraphJournalStressTest(ITestOutputHelper logOutput) => _logOutput = logOutput;

    private async Task<IHost> CreateService(bool useDatalake)
    {
        DatalakeOption datalakeOption = TestApplication.ReadDatalakeOption("test-GraphJournalStressTest");

        var host = Host.CreateDefaultBuilder()
            .ConfigureLogging(config =>
            {
                //config.AddFilter(x => true);
                config.AddLambda(x => _logOutput.WriteLine(x));
            })
            .ConfigureServices((context, services) =>
            {
                _ = useDatalake switch
                {
                    true => services.AddDatalakeFileStore(datalakeOption),
                    false => services.AddInMemoryFileStore(),
                };

                services.AddGraphEngine(config => config.BasePath = "basePath");

                services.AddKeyedKeyStore<DataETag>(FileSystemType.Hash, _dbCopyKeyedName, config =>
                {
                    config.BasePath = "db-recovery/data";
                    config.AddKeyStore();
                });
            })
            .Build();

        var context = host.Services.GetRequiredService<ILogger<GraphTransactionTests>>().ToScopeContext();

        IFileStore fileStore = host.Services.GetRequiredService<IFileStore>();
        await fileStore.ClearStore(context);

        IGraphEngine graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        await graphEngine.DataManager.LoadDatabase(context);

        return host;
    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(false, 2)]
    [InlineData(false, 4)]
    [InlineData(true, 1)]
    [InlineData(true, 2)]
    [InlineData(true, 4)]
    public async Task MultipleCommandStress(bool useDataLake, int taskCount)
    {
        using var host = await CreateService(useDataLake);
        var context = host.Services.GetRequiredService<ILogger<GraphTransactionTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var fileStore = host.Services.GetRequiredService<IFileStore>();
        var dbRecoveryFileStore = host.Services.GetRequiredKeyedService<IKeyStore<DataETag>>(_dbCopyKeyedName);

        var tracking = new ConcurrentQueue<int>();
        var nodesTracking = new ConcurrentQueue<(string node, Payload? payload)>();
        var edgesTracking = new ConcurrentQueue<(string from, string to)>();
        var taskMetrics = new ConcurrentQueue<(int taskNumber, TimeSpan timeSpan)>();

        int count = 0;
        var patterns = new (string name, Func<Task> func)[]
        {
            (nameof(singleNode), singleNode),
            (nameof(twoNodes), twoNodes),
            (nameof(twoNodesAndEdges), twoNodesAndEdges),
            (nameof(selectSingleNode), selectSingleNode),
            (nameof(selectSingleNodeWithData), selectSingleNodeWithData),
            (nameof(selectMultipleNodes), selectMultipleNodes),
            (nameof(selectMultipleNodesWithData), selectMultipleNodesWithData),
            (nameof(selectEdge), selectEdge),
        };

        var token = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        var start = DateTime.Now;
        var funcCount = 0;

        Func<int, Task> run = async taskNumber =>
        {
            while (!token.IsCancellationRequested)
            {
                int index = RandomNumberGenerator.GetInt32(patterns.Length);
                index.Assert(x => x <= patterns.Length);

                tracking.Enqueue(index);
                long timeStamp = Stopwatch.GetTimestamp();
                await patterns[index].func();
                var elapsed = Stopwatch.GetElapsedTime(timeStamp);
                taskMetrics.Enqueue((taskNumber, elapsed));

                Interlocked.Increment(ref funcCount);
                if (taskCount > 1) await Task.Yield();
            }
        };

        var tasks = Enumerable.Range(0, taskCount).Select(x => run(x)).ToArray();
        await Task.WhenAll(tasks);

        var totalSeconds = (DateTime.Now - start).TotalSeconds;
        var tsp = funcCount / totalSeconds;
        context.LogInformation("Total: TaskCount={taskCount}, commands={commands}, seconds={seconds}, tsp={tsp}", tasks.Length, funcCount, totalSeconds, tsp);

        taskMetrics.GroupBy(x => x.taskNumber)
            .Select(x => new { TaskNumber = x.Key, Count = x.Count(), TotalTime = x.Sum(y => y.timeSpan.TotalSeconds) })
            .OrderBy(x => x.TaskNumber)
            .ForEach(x => context.LogInformation("Task {TaskNumber}, executed={count}, seconds={seconds}", x.TaskNumber, x.Count, x.TotalTime));

        var trackingCounts = tracking.GroupBy(x => x)
            .Select(x => new { Index = x.Key, Count = x.Count() })
            .OrderBy(x => x.Index)
            .ForEach(x => context.LogInformation("Pattern '{Index}'={indexName}, Count={count} times, tps={tps}", x.Index, patterns[x.Index].name, x.Count, x.Count / totalSeconds));

        graphEngine.DataManager.GetMap().Action(x =>
        {
            context.LogInformation("Map: rows={rows}, edges={edges}, lastLogSequenceNumber={lastLogSequenceNumber}", x.Nodes.Count, x.Edges.Count, x.LastLogSequenceNumber);
        });

        await Stopwatch.GetTimestamp().Func(async timestamp =>
        {
            context.LogInformation("Recovering database...");

            var recoveredMapOption = await graphEngine.DataManager.BuildFromJournals(dbRecoveryFileStore, context);
            GraphMap recoveredMap = recoveredMapOption.BeOk().Return();

            recoveredMap.LastLogSequenceNumber.NotEmpty().Be(graphEngine.DataManager.GetMap().LastLogSequenceNumber);
            var compareMap = GraphCommandTools.CompareMap(recoveredMap, graphEngine.DataManager.GetMap());
            compareMap.Count.Be(0);

            int nodes = recoveredMap.Nodes.Count;
            int edges = recoveredMap.Edges.Count;
            int total = nodes + edges;

            TimeSpan elapsed = Stopwatch.GetElapsedTime(timestamp);
            double tps = total / elapsed.TotalSeconds;
            context.LogInformation("*** Recovered map, elapsed={elapsed}, nodes={nodes}, edges={edges}, total={total}, tps={tps}", elapsed, nodes, edges, total, tps);

            // Compare files
            var sourceFiles = await fileStore.Search("basepath/data/**/*", context);
            sourceFiles.Count.Assert(x => x > 0, "Source files should not be empty");

            var destinationFiles = await fileStore.Search("db-recovery/data/**/*", context);
            destinationFiles.Count.Assert(x => x > 0, "Destination files should not be empty");

            var missingFiles = sourceFiles.Select(x => removeFileSystem(x.Path))
                .Except(destinationFiles.Select(x => removeFileSystem(x.Path)))
                .ToArray();
            var missingFiles2 = destinationFiles.Select(x => removeFileSystem(x.Path))
                .Except(sourceFiles.Select(x => removeFileSystem(x.Path)))
                .ToArray();
            missingFiles
                .Assert(x => x.Length == 0, "Missing files should be empty");
            missingFiles2
                .Assert(x => x.Length == 0, "Missing files should be empty");

            sourceFiles.Count.Be(destinationFiles.Count, "Source and destination files should have the same count");

            var sourceFilesWithFileHashes = (await fileStore.GetFileHashes(sourceFiles, context)).BeOk().Return();
            var destinationFilesWithHashes = (await fileStore.GetFileHashes(destinationFiles, context)).BeOk().Return();
            sourceFilesWithFileHashes.Count.Be(destinationFilesWithHashes.Count);

            var leftOver = sourceFilesWithFileHashes.Select(x => removeFileSystem(x.Path) + ":" + x.ContentHash)
                .Except(destinationFilesWithHashes.Select(x => removeFileSystem(x.Path) + ":" + x.ContentHash))
                .ToArray()
                .Assert(x => x.Length == 0, "Left over files should be empty");

            string removeFileSystem(string filePath) => filePath
                .Split('/')
                .Skip(4)
                .Join('/');
        });

        string getNodeKey(string prefix) => $"{prefix}-{Interlocked.Increment(ref count)}";

        async Task singleNode()
        {
            string nodeKey = getNodeKey("N1");
            var payload = new Payload(nodeKey, count);
            var payloadBase64 = payload.ToJson().ToBase64();

            string cmd = $"add node key={nodeKey} set t1, data {{ '{payloadBase64}' }} ;";

            (await graphClient.ExecuteBatch(cmd, context)).BeOk().Return().Items.Count.Be(1);

            nodesTracking.Enqueue((nodeKey, payload));
        }

        async Task twoNodes()
        {
            string nodeKey = getNodeKey("N2");
            string nodeKey2 = getNodeKey("N2");

            string cmd = new[]
            {
                    $"add node key={nodeKey} set t1 ;",
                    $"add node key={nodeKey2} set t2 ;",
                }.Join(Environment.NewLine);

            (await graphClient.ExecuteBatch(cmd, context)).BeOk().Return().Items.Count.Be(2);

            nodesTracking.Enqueue((nodeKey, null));
            nodesTracking.Enqueue((nodeKey2, null));
        }

        async Task twoNodesAndEdges()
        {
            string nodeKey = getNodeKey("N2");
            string nodeKey2 = getNodeKey("N2");

            string cmd = new[]
            {
                    $"add node key={nodeKey} set t1 ;",
                    $"add node key={nodeKey2} set t2 ;",
                    $"add edge from={nodeKey}, to={nodeKey2}, type=default ;",
                }.Join(Environment.NewLine);

            (await graphClient.ExecuteBatch(cmd, context)).BeOk().Return().Items.Count.Be(3);

            nodesTracking.Enqueue((nodeKey, null));
            nodesTracking.Enqueue((nodeKey2, null));
            edgesTracking.Enqueue((nodeKey, nodeKey2));
        }

        async Task selectSingleNode()
        {
            var node = nodesTracking.Shuffle().FirstOrDefault();
            if (node == default) return;

            string cmd = $"select (key={node.node}) ; ";

            var result = (await graphClient.ExecuteBatch(cmd, context)).BeOk().Return();
            result.Items.Count.Be(1);
            result.Items[0].Action(x =>
            {
                x.Nodes.Count.Be(1);
                x.Edges.Count.Be(0);
                x.Nodes[0].Key.Be(node.node);
            });
        }

        async Task selectSingleNodeWithData()
        {
            (string node, Payload? payload) node = nodesTracking.Where(x => x.payload != null).Shuffle().FirstOrDefault();
            if (node == default) return;

            string cmd = $"select (key={node.node}) return data; ";

            var result = (await graphClient.ExecuteBatch(cmd, context)).BeOk().Return();
            result.Items.Count.Be(1);
            result.Items[0].Action(x =>
            {
                x.Nodes.Count.Be(1);
                x.Edges.Count.Be(0);

                if (node.payload != null)
                {
                    x.DataLinks.Count.Be(1);
                    x.DataLinks[0].Action(y =>
                    {
                        y.NodeKey.Be(node.node);
                        y.Name.Be("data");
                        y.Data.ToObject<Payload>().Assert(x => (x == node.payload));
                    });
                }
            });
        }

        async Task selectMultipleNodes()
        {
            if (nodesTracking.Count == 0) return;

            (string node, Payload? payload)[] sourceNodes = nodesTracking.Shuffle().Take(10).ToArray();
            string cmd = sourceNodes.Select((x, i) => $"select (key={x.node}) A{i} ; ").Join(Environment.NewLine);

            var result = (await graphClient.ExecuteBatch(cmd, context)).BeOk().Return();
            result.Items.Count.Be(sourceNodes.Length);
            result.Items.ForEach((x, i) =>
            {
                x.Nodes.Count.Be(1);
                x.Nodes[0].Key.Be(sourceNodes[i].node);
                x.Edges.Count.Be(0);
                x.DataLinks.Count.Be(0);
            });
        }

        async Task selectMultipleNodesWithData()
        {
            (string node, Payload? payload)[] sourceNodes = nodesTracking.Where(x => x.payload != null).Shuffle().Take(10).ToArray();
            if (sourceNodes.Length == 0) return;

            string cmd = sourceNodes.Select((x, i) => $"select (key={x.node}) A{i} return data ; ").Join(Environment.NewLine);

            var result = (await graphClient.ExecuteBatch(cmd, context)).BeOk().Return();
            result.Items.Count.Be(sourceNodes.Length);
            result.Items.ForEach((x, i) =>
            {
                x.Nodes.Count.Be(1);
                x.Nodes[0].Key.Be(sourceNodes[i].node);
                x.Edges.Count.Be(0);
                x.DataLinks.Action(y =>
                {
                    y.Count.Be(1);
                    y[0].Action(z =>
                    {
                        z.NodeKey.Be(sourceNodes[i].node);
                        z.Name.Be("data");
                        z.Data.ToObject<Payload>().Assert(p => p == sourceNodes[i].payload);
                    });
                });

            });
        }

        async Task selectEdge()
        {
            (string from, string to) edge = edgesTracking.Shuffle().FirstOrDefault();
            if (edge == default) return;

            string cmd = $"select [from={edge.from}, to={edge.to}, type=default] ; ";

            var result = (await graphClient.ExecuteBatch(cmd, context)).BeOk().Return();
            result.Items.Count.Be(1);
            result.Items[0].Action(x =>
            {
                x.Nodes.Count.Be(0);
                x.Edges.Count.Be(1);
                x.Edges[0].FromKey.Be(edge.from);
                x.Edges[0].ToKey.Be(edge.to);
                x.Edges[0].EdgeType.Be("default");
            });
        }
    }
}
