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
        GraphMap map = testClient.ServiceProvider.GetRequiredService<IGraphContext>().Map;

        string q = """
            add node key=node1;
            add node key=node2;
            add edge from=node1, to=node2, type=default;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Instance)).IsOk().Should().BeTrue();

        map.Nodes.Count.Should().Be(2);
        map.Edges.Count.Should().Be(1);

        string q2 = """
            add node key=node3;
            add edge from=node1, to=node2, type=default;
            """;

        (await testClient.ExecuteBatch(q2, NullScopeContext.Instance)).Action(x =>
        {
            x.IsError().Should().BeTrue();
            x.Value.Items.Count.Should().Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.Conflict));
        });
    }

    [Fact]
    public async Task UpdateNodeFailure()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost();
        GraphMap map = testClient.ServiceProvider.GetRequiredService<IGraphContext>().Map;

        string q = """
            add node key=node1;
            add node key=node2;
            set node key=node1 set t1;
            add edge from=node1, to=node2, type=default;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Instance)).IsOk().Should().BeTrue();

        map.Nodes.Count.Should().Be(2);
        map.Edges.Count.Should().Be(1);

        string q2 = """
            set node key=node2 set t2;
            add edge from=node1, to=node2, type=default;
            """;

        (await testClient.ExecuteBatch(q2, NullScopeContext.Instance)).Action(x =>
        {
            x.Value.Items.Count.Should().Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.Conflict));
        });
    }

    [Fact]
    public async Task DeleteNodeFailure()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost();
        GraphMap map = testClient.ServiceProvider.GetRequiredService<IGraphContext>().Map;

        string q = """
            add node key=node1;
            add node key=node2;
            add node key=node3;
            set node key=node1 set t1;
            add edge from=node1, to=node2, type=default;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Instance)).IsOk().Should().BeTrue();

        map.Nodes.Count.Should().Be(3);
        map.Edges.Count.Should().Be(1);

        string q2 = """
            delete node key=node3;
            add edge from=node1, to=node2, type=default;
            """;

        (await testClient.ExecuteBatch(q2, NullScopeContext.Instance)).Action(x =>
        {
            x.Value.Items.Count.Should().Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.Conflict));
        });
    }

    private void TestReturn(QueryResult graphResult, StatusCode statusCode)
    {
        graphResult.Option.StatusCode.Should().Be(statusCode, graphResult.Option.ToString());
    }
}
