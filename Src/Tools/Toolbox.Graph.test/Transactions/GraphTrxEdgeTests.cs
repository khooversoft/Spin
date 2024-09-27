using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Transactions;

public class GraphTrxEdgeTests
{

    [Fact]
    public async Task AddEdgeFailure()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost();
        GraphMap map = testClient.ServiceProvider.GetRequiredService<IGraphContext>().Map;

        string q = """
            add node key=node1;
            add node key=node2;
            add node key=node3;
            add edge from=node1, to=node2, type=default;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Instance)).IsOk().Should().BeTrue();

        map.Nodes.Count.Should().Be(3);
        map.Edges.Count.Should().Be(1);

        string q2 = """
            add edge from=node2, to=node3, type=default;
            add node key=node3;
            """;

        (await testClient.ExecuteBatch(q2, NullScopeContext.Instance)).Action(x =>
        {
            x.Value.Items.Count.Should().Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.Conflict));
        });

        map.Nodes.Count.Should().Be(3);
        map.Edges.Count.Should().Be(1);
    }

    [Fact]
    public async Task UpdateEdgeFailure()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost();
        GraphMap map = testClient.ServiceProvider.GetRequiredService<IGraphContext>().Map;

        string q = """
            add node key=node1;
            add node key=node2;
            add node key=node3;
            add edge from=node1, to=node2, type=default;
            add edge from=node2, to=node3, type=default;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Instance)).IsOk().Should().BeTrue();

        map.Nodes.Count.Should().Be(3);
        map.Edges.Count.Should().Be(2);

        string q2 = """
            set edge from=node2, to=node3, type=default set t1;
            add node key=node2;
            """;

        (await testClient.ExecuteBatch(q2, NullScopeContext.Instance)).Action(x =>
        {
            x.Value.Items.Count.Should().Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.Conflict));
        });

        map.Nodes.Count.Should().Be(3);
        map.Edges.Count.Should().Be(2);
    }

    [Fact]
    public async Task DeleteEdgeFailure()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost();
        GraphMap map = testClient.ServiceProvider.GetRequiredService<IGraphContext>().Map;

        string q = """
            add node key=node1;
            add node key=node2;
            add node key=node3;
            set node key=node1 set t1;
            add edge from=node1, to=node2, type=default;
            add edge from=node2, to=node3, type=default;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Instance)).IsOk().Should().BeTrue();

        map.Nodes.Count.Should().Be(3);
        map.Edges.Count.Should().Be(2);

        string q2 = """
            delete edge from=node2, to=node3, type=default;
            add edge from=node1, to=node2, type=default;
            """;

        (await testClient.ExecuteBatch(q2, NullScopeContext.Instance)).Action(x =>
        {
            x.Value.Items.Count.Should().Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.Conflict));
        });


        map.Nodes.Count.Should().Be(3);
        map.Edges.Count.Should().Be(2);
    }

    private void TestReturn(QueryResult graphResult, StatusCode statusCode)
    {
        graphResult.Option.StatusCode.Should().Be(statusCode, graphResult.Option.ToString());
    }
}
