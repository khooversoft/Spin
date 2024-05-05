using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Transactions;

public class GraphTrxNodeTests
{

    [Fact]
    public async Task AddNodeFailure()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost();
        var map = testClient.ServiceProvider.GetRequiredService<GraphMap>();

        string q = """
            add node key=node1;
            add node key=node2;
            add unique edge fromKey=node1, toKey=node2;
            """;

        (await testClient.Execute(q, NullScopeContext.Instance)).IsOk().Should().BeTrue();

        map.Nodes.Count.Should().Be(2);
        map.Edges.Count.Should().Be(1);

        string q2 = """
            add node key=node3;
            add unique edge fromKey=node1, toKey=node2;
            """;

        (await testClient.Execute(q2, NullScopeContext.Instance)).Action(x =>
        {
            x.IsError().Should().BeTrue();
            x.Value.Items.Length.Should().Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, CommandType.AddNode, StatusCode.OK, 0));
            x.Value.Items[1].Action(y => TestReturn(y, CommandType.AddEdge, StatusCode.Conflict, 0));
        });
    }

    [Fact]
    public async Task UpdateNodeFailure()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost();
        var map = testClient.ServiceProvider.GetRequiredService<GraphMap>();

        string q = """
            add node key=node1;
            add node key=node2;
            update (key=node1) set t1;
            add unique edge fromKey=node1, toKey=node2;
            """;

        (await testClient.Execute(q, NullScopeContext.Instance)).IsOk().Should().BeTrue();

        map.Nodes.Count.Should().Be(2);
        map.Edges.Count.Should().Be(1);

        string q2 = """
            update (key=node2) set t2;
            add unique edge fromKey=node1, toKey=node2;
            """;

        (await testClient.Execute(q2, NullScopeContext.Instance)).Action(x =>
        {
            x.Value.Items.Length.Should().Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, CommandType.UpdateNode, StatusCode.OK, 1));
            x.Value.Items[1].Action(y => TestReturn(y, CommandType.AddEdge, StatusCode.Conflict, 0));
        });
    }

    [Fact]
    public async Task DeleteNodeFailure()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost();
        var map = testClient.ServiceProvider.GetRequiredService<GraphMap>();

        string q = """
            add node key=node1;
            add node key=node2;
            add node key=node3;
            update (key=node1) set t1;
            add unique edge fromKey=node1, toKey=node2;
            """;

        (await testClient.Execute(q, NullScopeContext.Instance)).IsOk().Should().BeTrue();

        map.Nodes.Count.Should().Be(3);
        map.Edges.Count.Should().Be(1);

        string q2 = """
            delete (key=node3);
            add unique edge fromKey=node1, toKey=node2;
            """;

        (await testClient.Execute(q2, NullScopeContext.Instance)).Action(x =>
        {
            x.Value.Items.Length.Should().Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, CommandType.DeleteNode, StatusCode.OK, 1));
            x.Value.Items[1].Action(y => TestReturn(y, CommandType.AddEdge, StatusCode.Conflict, 0));
        });
    }

    private void TestReturn(GraphQueryResult graphResult, CommandType commandType, StatusCode statusCode, int itemCount)
    {
        graphResult.CommandType.Should().Be(commandType);
        graphResult.Status.StatusCode.Should().Be(statusCode, graphResult.Status.ToString());
        graphResult.Items.Length.Should().Be(itemCount);
    }
}
