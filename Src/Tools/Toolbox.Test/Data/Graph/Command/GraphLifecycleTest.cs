using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Test.Data.Graph.Command;

public class GraphLifecycleTest
{
    [Fact]
    public void SingleNode()
    {
        GraphMap map = new GraphMap();

        Option<GraphQueryResults> addResult = map.Execute("add node key=node1, t1,t2=v1;");
        addResult.IsOk().Should().BeTrue();
        map.Nodes.Count.Should().Be(1);
        map.Edges.Count.Should().Be(0);

        map.ExecuteScalar("select (key=node1);").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Items.Count.Should().Be(1);
            x.Items.OfType<GraphNode>().First().Action(x =>
            {
                x.Key.Should().Be("node1");
                x.Tags.ToString().Should().Be("t1,t2=v1");
            });
        });

        Option<GraphQueryResults> removeResult = map.Execute("delete (key=node1);");
        removeResult.IsOk().Should().BeTrue(removeResult.ToString());
        map.Nodes.Count.Should().Be(0);
        map.Edges.Count.Should().Be(0);

        map.ExecuteScalar("select (key=node1);").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Items.Count.Should().Be(0);
        });
    }

    [Fact]
    public void TwoNodes()
    {
        GraphMap map = new GraphMap();

        Option<GraphQueryResults> addResult1 = map.Execute("add node key=node1, t1,t2=v1;");
        addResult1.IsOk().Should().BeTrue();
        map.Nodes.Count.Should().Be(1);
        map.Edges.Count.Should().Be(0);

        Option<GraphQueryResults> addResult2 = map.Execute("add node key=node2, t10,t20=v10;");
        addResult2.IsOk().Should().BeTrue();
        map.Nodes.Count.Should().Be(2);
        map.Edges.Count.Should().Be(0);

        map.ExecuteScalar("select (key=node1);").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Items.Count.Should().Be(1);
            x.Items.OfType<GraphNode>().First().Action(x =>
            {
                x.Key.Should().Be("node1");
                x.Tags.ToString().Should().Be("t1,t2=v1");
            });
        });

        map.ExecuteScalar("select (key=node2);").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Items.Count.Should().Be(1);
            x.Items.OfType<GraphNode>().First().Action(x =>
            {
                x.Key.Should().Be("node2");
                x.Tags.ToString().Should().Be("t10,t20=v10");
            });
        });

        map.Execute("delete (key=node1);").Action(x =>
        {
            x.IsOk().Should().BeTrue(x.ToString());
            map.Nodes.Count.Should().Be(1);
            map.Edges.Count.Should().Be(0);
        });

        map.ExecuteScalar("select (key=node1);").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Items.Count.Should().Be(0);
        });

        map.ExecuteScalar("select (key=node2);").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Items.Count.Should().Be(1);
            x.Items.OfType<GraphNode>().First().Action(x =>
            {
                x.Key.Should().Be("node2");
                x.Tags.ToString().Should().Be("t10,t20=v10");
            });
        });

        map.Execute("delete (key=node2);").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Items.Count.Should().Be(1);
        });

        map.Nodes.Count.Should().Be(0);
        map.Edges.Count.Should().Be(0);
    }

    [Fact]
    public void FailOnDuplicateTagKey()
    {
        GraphMap map = new GraphMap();

        string q = """
            add node key=node1, t1;
            add node key=node1,t2,client;
            """;

        map.Execute(q).Action(x =>
        {
            x.IsError().Should().BeTrue();
        });

        map.Nodes.Count.Should().Be(1);
        map.Edges.Count.Should().Be(0);
    }

    [Fact]
    public void TwoNodesWithRelationship()
    {
        GraphMap map = new GraphMap();

        string q = """
            add node key=node1, t1;
            add node key=node2,t2,client;
            add edge fromKey=node1,toKey=node2,edgeType=et,e2, worksFor;
            """;

        map.Execute(q).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Items.Count.Should().Be(3);
        });

        map.Nodes.Count.Should().Be(2);
        map.Edges.Count.Should().Be(1);

        var query = map.ExecuteScalar("select (key=node1) a0 -> [*] a1 -> (*) a2;");
        query.IsOk().Should().Be(true);
        query.Items.Count.Should().Be(2);
        query.Items.OfType<GraphNode>().Action(x =>
        {
            x.Count().Should().Be(2);
            x.First().Key.Should().Be("node1");
            x.Last().Key.Should().Be("node2");
        });
    }

    [Fact]
    public void TwoNodesWithRelationshipLargerSet()
    {
        GraphMap map = new GraphMap();

        string q = """
            add node key=node1,t1;
            add node key=node2,t2,client;
            add node key=node3,t3,client;
            add node key=node4,t4,client;
            add node key=node5,t5,client;
            add edge fromKey=node1,toKey=node2,edgeType=et,e2,worksFor;
            add edge fromKey=node3,toKey=node4,edgeType=et,e2,worksFor;
            """;

        map.Execute(q).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Items.Count.Should().Be(7);
        });

        map.Nodes.Count.Should().Be(5);
        map.Edges.Count.Should().Be(2);

        map.ExecuteScalar("select (key=node3) a0 -> [*] a1 -> (*) a2;").Action(x =>
        {
            x.IsOk().Should().Be(true);
            x.Items.OfType<GraphNode>().Action(x =>
            {
                x.Count().Should().Be(2);
                x.First().Key.Should().Be("node3");
                x.Last().Key.Should().Be("node4");
            });
        });
    }
}
