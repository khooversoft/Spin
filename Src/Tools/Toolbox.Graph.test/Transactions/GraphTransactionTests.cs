using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Journal;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Transactions;

public class GraphTransactionTests
{
    [Fact]
    public async Task SimpleSetOfCommandsWithFailuresKey()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService();

        IFileStore fileStore = testClient.Services.GetRequiredService<IFileStore>();
        IJournalFile transactionLog = testClient.Services.GetRequiredKeyedService<IJournalFile>(GraphConstants.TrxJournal.DiKeyed);
        var context = testClient.CreateScopeContext<GraphTransactionTests>();
        int expectedJournalCount = 0;

        string q = """
            add node key=node1 set t1;
            add node key=node2 set t2,client;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Default)).Action(x =>
        {
            x.IsOk().BeTrue(x.ToString());
            x.Value.Items.Count.Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.OK));
        });

        testClient.Map.Nodes.Count.Be(2);
        testClient.Map.Edges.Count.Be(0);

        expectedJournalCount += 4;
        IReadOnlyList<JournalEntry> journals = await transactionLog.ReadJournals(context);
        journals.Count.Be(expectedJournalCount);

        (await testClient.ExecuteBatch("add node key=node1 set t1, t2;", NullScopeContext.Default)).Action(x =>
        {
            x.IsConflict().BeTrue(x.ToString());
            x.Value.Items.Count.Be(1);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.Conflict));
        });

        testClient.Map.Nodes.Count.Be(2);
        testClient.Map.Edges.Count.Be(0);

        journals = await transactionLog.ReadJournals(context);
        journals.Count.Be(expectedJournalCount);

        string q2 = """
            add node key=node3 set t1;
            add edge from=node1, to=node2, type=default;
            add node key=node4 set t2,client;
            add edge from=node3, to=node4, type=default;
            """;

        (await testClient.ExecuteBatch(q2, NullScopeContext.Default)).Action(x =>
        {
            x.IsOk().BeTrue(x.ToString());
            x.Value.Items.Count.Be(4);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[2].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[3].Action(y => TestReturn(y, StatusCode.OK));
        });

        testClient.Map.Nodes.Count.Be(4);
        testClient.Map.Edges.Count.Be(2);

        expectedJournalCount += 6;
        journals = await transactionLog.ReadJournals(context);
        journals.Count.Be(expectedJournalCount);

        string q3 = """
            add node key=node5 set t1;
            add edge from=node2, to=node4, type=default;
            add node key=node2;
            add node key=node6 set t2,client;
            add edge from=node3, to=node4, type=default;
            """;

        (await testClient.ExecuteBatch(q3, NullScopeContext.Default)).Action(x =>
        {
            x.IsError().BeTrue(x.ToString());
            x.Value.Items.Count.Be(3);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[2].Action(y => TestReturn(y, StatusCode.Conflict));
        });

        journals = await transactionLog.ReadJournals(context);
        journals.Count.Be(expectedJournalCount);

        testClient.Map.Nodes.Count.Be(4);
        testClient.Map.Nodes.OrderBy(x => x.Key).Select(x => x.Key).SequenceEqual(["node1", "node2", "node3", "node4"]).BeTrue();

        testClient.Map.Edges.Count.Be(2);
        testClient.Map.Edges.Select(x => (x.FromKey, x.ToKey)).OrderBy(x => x).SequenceEqual([("node1", "node2"), ("node3", "node4")]).BeTrue();
    }

    private void TestReturn(QueryResult graphResult, StatusCode statusCode)
    {
        graphResult.Option.StatusCode.Be(statusCode, graphResult.Option.ToString());
    }
}
