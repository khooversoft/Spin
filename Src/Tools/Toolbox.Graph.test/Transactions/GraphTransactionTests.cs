using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Azure;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.Models;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Transactions;

public class GraphTransactionTests
{
    private readonly ITestOutputHelper _logOutput;
    public GraphTransactionTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

    private async Task<IHost> CreateService(bool useDatalake)
    {
        DatalakeOption datalakeOption = TestApplication.ReadDatalakeOption("test-GraphTransactionTests");

        var host = Host.CreateDefaultBuilder()
            .ConfigureLogging(config => config.AddFilter(x => true).AddLambda(x => _logOutput.WriteLine(x)))
            .ConfigureServices((context, services) =>
            {
                _ = useDatalake switch
                {
                    true => services.AddDatalakeFileStore(datalakeOption),
                    false => services.AddInMemoryFileStore(),
                };

                services.AddGraphEngine(config => config.BasePath = "basePath");
            })
            .Build();

        var context = host.Services.GetRequiredService<ILogger<GraphTransactionTests>>().ToScopeContext();

        IFileStore fileStore = host.Services.GetRequiredService<IFileStore>();
        var list = await fileStore.Search("**/*", context);
        await list.ForEachAsync(async x => await fileStore.File(x.Path).Delete(context));

        IGraphEngine graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        await graphEngine.DataManager.LoadDatabase(context);

        return host;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SimpleSetOfCommandsWithFailuresKey(bool useDataLake)
    {
        using var host = await CreateService(useDataLake);
        var context = host.Services.GetRequiredService<ILogger<GraphTransactionTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        IDataListClient<DataChangeRecord> changeClient = host.Services.GetRequiredService<IDataListClient<DataChangeRecord>>();

        string q = """
            add node key=node1 set t1;
            add node key=node2 set t2,client;
            """;

        (await graphClient.ExecuteBatch(q, context)).Action(x =>
        {
            x.IsOk().BeTrue(x.ToString());
            x.Value.Items.Count.Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.OK));
        });

        graphEngine.DataManager.GetMap().Nodes.Count.Be(2);
        graphEngine.DataManager.GetMap().Edges.Count.Be(0);

        (await changeClient.Get(GraphConstants.Journal.Key, "**/*", context)).Action(x =>
        {
            x.BeOk();
            IReadOnlyList<DataChangeRecord> journals = x.Return();

            journals.Count.Be(1);
            var entries = GetChangeRecords(journals);
            entries.Count.Be(2);
            var cursor = entries.ToCursor();

            ValidateNode1(cursor);
            ValidateNode2(cursor);
        });

        (await graphClient.ExecuteBatch("add node key=node1 set t1, t2;", context)).Action(x =>
        {
            x.IsConflict().BeTrue(x.ToString());
            x.Value.Items.Count.Be(1);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.Conflict));
        });

        graphEngine.DataManager.GetMap().Nodes.Count.Be(2);
        graphEngine.DataManager.GetMap().Edges.Count.Be(0);

        (await changeClient.Get(GraphConstants.Journal.Key, "**/*", context)).Action(x =>
        {
            x.BeOk();
            IReadOnlyList<DataChangeRecord> journals = x.Return();

            journals.Count.Be(1);
            var entries = GetChangeRecords(journals);
            entries.Count.Be(2);
            var cursor = entries.ToCursor();

            ValidateNode1(cursor);
            ValidateNode2(cursor);
        });

        string q2 = """
            add node key=node3 set t1;
            add edge from=node1, to=node2, type=default;
            add node key=node4 set t2,client;
            add edge from=node3, to=node4, type=subscription;
            """;

        (await graphClient.ExecuteBatch(q2, context)).Action(x =>
        {
            x.IsOk().BeTrue(x.ToString());
            x.Value.Items.Count.Be(4);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[2].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[3].Action(y => TestReturn(y, StatusCode.OK));
        });

        graphEngine.DataManager.GetMap().Nodes.Count.Be(4);
        graphEngine.DataManager.GetMap().Edges.Count.Be(2);

        (await changeClient.Get(GraphConstants.Journal.Key, "**/*", context)).Action(x =>
        {
            x.BeOk();
            IReadOnlyList<DataChangeRecord> journals = x.Return();

            journals.Count.Be(2);
            var entries = GetChangeRecords(journals);
            entries.Count.Be(6);
            var cursor = entries.ToCursor();

            ValidateNode1(cursor);
            ValidateNode2(cursor);
            ValidateNode3(cursor);
            ValidateEdge1_2(cursor);
            ValidateNode4(cursor);
            ValidateEdge3_4(cursor);
        });

        graphEngine.DataManager.GetMap().Nodes.Count.Be(4);

        graphEngine.DataManager.GetMap().Nodes
            .OrderBy(x => x.Key)
            .Select(x => x.Key)
            .SequenceEqual(["node1", "node2", "node3", "node4"])
            .BeTrue();

        graphEngine.DataManager.GetMap().Edges.Count.Be(2);

        graphEngine.DataManager.GetMap().Edges
            .Select(x => (x.FromKey, x.ToKey))
            .OrderBy(x => x)
            .SequenceEqual([("node1", "node2"), ("node3", "node4")])
            .BeTrue();
    }

    private void ValidateNode1(Cursor<DataChangeEntry> cursor)
    {
        cursor.NextValue().BeOk().Return().Action(x =>
        {
            x.TypeName.Be(typeof(GraphNode).Name);
            x.SourceName.Be(ChangeSource.Node);
            x.ObjectId.Be("node1");
            x.Action.Be(ChangeOperation.Add);
            x.Before.BeNull();
            x.After.NotNull();

            GraphNode node = x.After.Value.ToObject<GraphNode>();
            node.Key.Be("node1");
            node.Tags.Count.Be(1);
            node.Tags.ContainsKey("t1").BeTrue();
        });
    }

    private void ValidateNode2(Cursor<DataChangeEntry> cursor)
    {
        cursor.NextValue().BeOk().Return().Action(x =>
        {
            x.TypeName.Be(typeof(GraphNode).Name);
            x.SourceName.Be(ChangeSource.Node);
            x.ObjectId.Be("node2");
            x.Action.Be(ChangeOperation.Add);
            x.Before.BeNull();
            x.After.NotNull();

            GraphNode node = x.After.Value.ToObject<GraphNode>();
            node.Key.Be("node2");
            node.Tags.Count.Be(2);
            node.Tags.ContainsKey("t2").BeTrue();
            node.Tags.ContainsKey("client").BeTrue();
        });
    }

    private void ValidateNode3(Cursor<DataChangeEntry> cursor)
    {
        cursor.NextValue().BeOk().Return().Action(x =>
        {
            x.TypeName.Be(typeof(GraphNode).Name);
            x.SourceName.Be(ChangeSource.Node);
            x.ObjectId.Be("node3");
            x.Action.Be(ChangeOperation.Add);
            x.Before.BeNull();
            x.After.NotNull();

            GraphNode node = x.After.Value.ToObject<GraphNode>();
            node.Key.Be("node3");
            node.Tags.Count.Be(1);
            node.Tags.ContainsKey("t1").BeTrue();
        });
    }

    private void ValidateEdge1_2(Cursor<DataChangeEntry> cursor)
    {
        cursor.NextValue().BeOk().Return().Action(x =>
        {
            x.TypeName.Be(typeof(GraphEdge).Name);
            x.SourceName.Be(ChangeSource.Edge);
            x.ObjectId.Be("node1->node2(default)");
            x.Action.Be(ChangeOperation.Add);
            x.Before.BeNull();
            x.After.NotNull();

            GraphEdge edge = x.After.Value.ToObject<GraphEdge>();
            edge.FromKey.Be("node1");
            edge.ToKey.Be("node2");
            edge.EdgeType.Be("default");
            edge.Tags.Count.Be(0);
        });
    }

    private void ValidateNode4(Cursor<DataChangeEntry> cursor)
    {
        cursor.NextValue().BeOk().Return().Action(x =>
        {
            x.TypeName.Be(typeof(GraphNode).Name);
            x.SourceName.Be(ChangeSource.Node);
            x.ObjectId.Be("node4");
            x.Action.Be(ChangeOperation.Add);
            x.Before.BeNull();
            x.After.NotNull();

            GraphNode node = x.After.Value.ToObject<GraphNode>();
            node.Key.Be("node4");
            node.Tags.Count.Be(2);
            node.Tags.ContainsKey("t2").BeTrue();
            node.Tags.ContainsKey("client").BeTrue();
        });
    }

    private void ValidateEdge3_4(Cursor<DataChangeEntry> cursor)
    {
        cursor.NextValue().BeOk().Return().Action(x =>
        {
            x.TypeName.Be(typeof(GraphEdge).Name);
            x.SourceName.Be(ChangeSource.Edge);
            x.ObjectId.Be("node3->node4(subscription)");
            x.Action.Be(ChangeOperation.Add);
            x.Before.BeNull();
            x.After.NotNull();

            GraphEdge edge = x.After.Value.ToObject<GraphEdge>();
            edge.FromKey.Be("node3");
            edge.ToKey.Be("node4");
            edge.EdgeType.Be("subscription");
            edge.Tags.Count.Be(0);
        });
    }

    private void TestReturn(QueryResult graphResult, StatusCode statusCode)
    {
        graphResult.Option.StatusCode.Be(statusCode, graphResult.Option.ToString());
    }

    private static IReadOnlyList<DataChangeEntry> GetChangeRecords(IReadOnlyList<DataChangeRecord> subject)
    {
        subject.NotNull().ForEach(x =>
        {
            x.Validate().ThrowOnError();
            x.Entries.All(y => y.TransactionId == x.TransactionId).BeTrue();
        });

        var result = subject.SelectMany(x => x.Entries).OrderBy(x => x.LogSequenceNumber).ToArray();
        return result;
    }
}
