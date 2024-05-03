using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Transactions;

public class GraphTrxEdgeTests
{

    [Fact]
    public void AddEdgeFailure()
    {
        GraphMap map = new GraphMap();

        string q = """
            add node key=node1;
            add node key=node2;
            add node key=node3;
            add unique edge fromKey=node1, toKey=node2;
            """;

        map.Execute(q, NullScopeContext.Instance).IsOk().Should().BeTrue();

        map.Nodes.Count.Should().Be(3);
        map.Edges.Count.Should().Be(1);

        string q2 = """
            add unique edge fromKey=node2, toKey=node3;
            add node key=node3;
            """;

        map.Execute(q2, NullScopeContext.Instance).Action(x =>
        {
            x.Value.Items.Length.Should().Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, CommandType.AddEdge, StatusCode.OK, 0));
            x.Value.Items[1].Action(y => TestReturn(y, CommandType.AddNode, StatusCode.Conflict, 0));
        });

        map.Nodes.Count.Should().Be(3);
        map.Edges.Count.Should().Be(1);
    }

    [Fact]
    public void UpdateEdgeFailure()
    {
        GraphMap map = new GraphMap();

        string q = """
            add node key=node1;
            add node key=node2;
            add node key=node3;
            add unique edge fromKey=node1, toKey=node2;
            add unique edge fromKey=node2, toKey=node3;
            """;

        map.Execute(q, NullScopeContext.Instance).IsOk().Should().BeTrue();

        map.Nodes.Count.Should().Be(3);
        map.Edges.Count.Should().Be(2);

        string q2 = """
            update [fromKey=node2, toKey=node3] set t1;
            add node key=node2;
            """;

        map.Execute(q2, NullScopeContext.Instance).Action(x =>
        {
            x.Value.Items.Length.Should().Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, CommandType.UpdateEdge, StatusCode.OK, 1));
            x.Value.Items[1].Action(y => TestReturn(y, CommandType.AddNode, StatusCode.Conflict, 0));
        });

        map.Nodes.Count.Should().Be(3);
        map.Edges.Count.Should().Be(2);
    }

    [Fact]
    public void DeleteEdgeFailure()
    {
        GraphMap map = new GraphMap();

        string q = """
            add node key=node1;
            add node key=node2;
            add node key=node3;
            update (key=node1) set t1;
            add unique edge fromKey=node1, toKey=node2;
            add unique edge fromKey=node2, toKey=node3;
            """;

        map.Execute(q, NullScopeContext.Instance).Action(x => x.IsOk().Should().BeTrue(x.ToString()));

        map.Nodes.Count.Should().Be(3);
        map.Edges.Count.Should().Be(2);

        string q2 = """
            delete [fromKey=node2, toKey=node3];
            add unique edge fromKey=node1, toKey=node2;
            """;

        map.Execute(q2, NullScopeContext.Instance).Action(x =>
        {
            x.Value.Items.Length.Should().Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, CommandType.DeleteEdge, StatusCode.OK, 1));
            x.Value.Items[1].Action(y => TestReturn(y, CommandType.AddEdge, StatusCode.Conflict, 0));
        });


        map.Nodes.Count.Should().Be(3);
        map.Edges.Count.Should().Be(2);
    }

    private void TestReturn(GraphQueryResult graphResult, CommandType commandType, StatusCode statusCode, int itemCount)
    {
        graphResult.CommandType.Should().Be(commandType);
        graphResult.Status.StatusCode.Should().Be(statusCode, graphResult.Status.ToString());
        graphResult.Items.Length.Should().Be(itemCount);
    }
}
