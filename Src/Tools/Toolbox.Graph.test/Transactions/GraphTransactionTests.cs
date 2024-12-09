using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.TransactionLog;
using Toolbox.Types;

namespace Toolbox.Graph.test.Transactions;

public class GraphTransactionTests
{
    [Fact]
    public async Task SimpleSetOfCommandsWithFailuresKey()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost();
        GraphMap map = testClient.ServiceProvider.GetRequiredService<IGraphHost>().Map;
        IFileStore fileStore = testClient.ServiceProvider.GetRequiredService<IFileStore>();
        ITransactionLog transactionLog = testClient.ServiceProvider.GetRequiredService<ITransactionLog>();
        ILogger<GraphTransactionTests> logger = testClient.ServiceProvider.GetRequiredService<ILogger<GraphTransactionTests>>();
        int expectedJournalCount = 0;

        var context = new ScopeContext(logger);

        string q = """
            add node key=node1 set t1;
            add node key=node2 set t2,client;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Default)).Action(x =>
        {
            x.IsOk().Should().BeTrue(x.ToString());
            x.Value.Items.Count.Should().Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.OK));
        });

        map.Nodes.Count.Should().Be(2);
        map.Edges.Count.Should().Be(0);

        expectedJournalCount += 3;
        IReadOnlyList<JournalEntry> journals = await transactionLog.ReadJournals(GraphConstants.JournalName, context);
        journals.Count.Should().Be(expectedJournalCount);

        (await testClient.ExecuteBatch("add node key=node1 set t1, t2;", NullScopeContext.Default)).Action(x =>
        {
            x.IsConflict().Should().BeTrue(x.ToString());
            x.Value.Items.Count.Should().Be(1);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.Conflict));
        });

        map.Nodes.Count.Should().Be(2);
        map.Edges.Count.Should().Be(0);

        journals = await transactionLog.ReadJournals(GraphConstants.JournalName, context);
        journals.Count.Should().Be(expectedJournalCount);

        string q2 = """
            add node key=node3 set t1;
            add edge from=node1, to=node2, type=default;
            add node key=node4 set t2,client;
            add edge from=node3, to=node4, type=default;
            """;

        (await testClient.ExecuteBatch(q2, NullScopeContext.Default)).Action(x =>
        {
            x.IsOk().Should().BeTrue(x.ToString());
            x.Value.Items.Count.Should().Be(4);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[2].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[3].Action(y => TestReturn(y, StatusCode.OK));
        });

        map.Nodes.Count.Should().Be(4);
        map.Edges.Count.Should().Be(2);

        expectedJournalCount += 5;
        journals = await transactionLog.ReadJournals(GraphConstants.JournalName, context);
        journals.Count.Should().Be(expectedJournalCount);

        string q3 = """
            add node key=node5 set t1;
            add edge from=node2, to=node4, type=default;
            add node key=node2;
            add node key=node6 set t2,client;
            add edge from=node3, to=node4, type=default;
            """;

        (await testClient.ExecuteBatch(q3, NullScopeContext.Default)).Action(x =>
        {
            x.IsError().Should().BeTrue(x.ToString());
            x.Value.Items.Count.Should().Be(3);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[2].Action(y => TestReturn(y, StatusCode.Conflict));
        });

        journals = await transactionLog.ReadJournals(GraphConstants.JournalName, context);
        journals.Count.Should().Be(expectedJournalCount);

        map.Nodes.Count.Should().Be(4);
        map.Nodes.OrderBy(x => x.Key).Select(x => x.Key).Should().BeEquivalentTo(["node1", "node2", "node3", "node4"]);

        map.Edges.Count.Should().Be(2);
        map.Edges.Select(x => (x.FromKey, x.ToKey)).Should().BeEquivalentTo([("node1", "node2"), ("node3", "node4")]);
    }

    private void TestReturn(QueryResult graphResult, StatusCode statusCode)
    {
        graphResult.Option.StatusCode.Should().Be(statusCode, graphResult.Option.ToString());
    }
}
