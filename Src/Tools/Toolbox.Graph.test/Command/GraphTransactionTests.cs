﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class GraphTransactionTests
{
    [Fact]
    public void FailOnDuplicateTagKey()
    {
        GraphMap map = new GraphMap();

        string q = """
            add node key=node1, t1;
            add node key=node2,t2,client;
            """;

        map.Execute(q, NullScopeContext.Instance).Action(x =>
        {
            x.IsOk().Should().BeTrue(x.ToString());
            x.Value.Items.Count.Should().Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, CommandType.AddNode, StatusCode.OK, 0));
            x.Value.Items[1].Action(y => TestReturn(y, CommandType.AddNode, StatusCode.OK, 0));
        });

        map.Nodes.Count.Should().Be(2);
        map.Edges.Count.Should().Be(0);

        map.Execute("add node key=node1, t1, t2;", NullScopeContext.Instance).Action(x =>
        {
            x.IsConflict().Should().BeTrue(x.ToString());
            x.Value.Items.Count.Should().Be(1);
            x.Value.Items[0].Action(y => TestReturn(y, CommandType.AddNode, StatusCode.Conflict, 0));
        });

        map.Nodes.Count.Should().Be(2);
        map.Edges.Count.Should().Be(0);

        string q2 = """
            add node key=node3, t1;
            add edge fromKey=node1, toKey=node2;
            add node key=node4,t2,client;
            add edge fromKey=node3, toKey=node4;
            """;

        map.Execute(q2, NullScopeContext.Instance).Action(x =>
        {
            x.IsOk().Should().BeTrue(x.ToString());
            x.Value.Items.Count.Should().Be(4);
            x.Value.Items[0].Action(y => TestReturn(y, CommandType.AddNode, StatusCode.OK, 0));
            x.Value.Items[1].Action(y => TestReturn(y, CommandType.AddEdge, StatusCode.OK, 0));
            x.Value.Items[2].Action(y => TestReturn(y, CommandType.AddNode, StatusCode.OK, 0));
            x.Value.Items[3].Action(y => TestReturn(y, CommandType.AddEdge, StatusCode.OK, 0));
        });

        map.Nodes.Count.Should().Be(4);
        map.Edges.Count.Should().Be(2);

        string q3 = """
            add node key=node5, t1;
            add edge fromKey=node2, toKey=node4;
            add node key=node2;
            add node key=node6,t2,client;
            add edge fromKey=node3, toKey=node4;
            """;

        map.Execute(q3, NullScopeContext.Instance).Action(x =>
        {
            x.IsError().Should().BeTrue(x.ToString());
            x.Value.Items.Count.Should().Be(3);
            x.Value.Items[0].Action(y => TestReturn(y, CommandType.AddNode, StatusCode.OK, 0));
            x.Value.Items[1].Action(y => TestReturn(y, CommandType.AddEdge, StatusCode.OK, 0));
            x.Value.Items[2].Action(y => TestReturn(y, CommandType.AddNode, StatusCode.Conflict, 0));
        });

        map.Nodes.Count.Should().Be(4);
        map.Nodes.OrderBy(x => x.Key).Select(x => x.Key).Should().BeEquivalentTo(["node1", "node2", "node3", "node4"]);

        map.Edges.Count.Should().Be(2);
        map.Edges.Select(x => (x.FromKey, x.ToKey)).Should().BeEquivalentTo([("node1", "node2"), ("node3", "node4")]);
    }

    private void TestReturn(GraphQueryResult graphResult, CommandType commandType, StatusCode statusCode, int itemCount)
    {
        graphResult.CommandType.Should().Be(commandType);
        graphResult.Status.StatusCode.Should().Be(statusCode, graphResult.Status.ToString());
        graphResult.Items.Count.Should().Be(itemCount);
    }
}
