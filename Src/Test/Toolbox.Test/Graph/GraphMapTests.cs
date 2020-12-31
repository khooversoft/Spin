using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Graph;
using Xunit;

namespace Toolbox.Test.Graph
{
    public class GraphTests
    {
        [Fact]
        public void EmptyNodeTest()
        {
            var map = new GraphMap<string, IGraphNode<string>, IGraphEdge<string>>();
        }

        [Fact]
        public void OneNodeTest()
        {
            var map = new GraphMap<string, IGraphNode<string>, IGraphEdge<string>>()
            {
                new GraphNode<string>("Node1"),
            };

            map.Nodes.Count.Should().Be(1);
            map.Edges.Count.Should().Be(0);
        }

        [Fact]
        public void TwoNodesSameKeyOverwriteTest()
        {
            GraphMap<string, IGraphNode<string>, IGraphEdge<string>> map = null!;

            map = new GraphMap<string, IGraphNode<string>, IGraphEdge<string>>()
            {
                new GraphNode<string>("Node1"),
                new GraphNode<string>("Node1"),
            };

            map.Nodes.Count.Should().Be(1);
            map.Edges.Count.Should().Be(0);
        }

        [Fact]
        public void TwoNodesTest()
        {
            var map = new GraphMap<string, IGraphNode<string>, IGraphEdge<string>>()
            {
                new GraphNode<string>("Node1"),
                new GraphNode<string>("Node2"),
            };

            map.Nodes.Count.Should().Be(2);
            map.Edges.Count.Should().Be(0);
        }

        [Fact]
        public void TwoNodesCountTest()
        {
            var map = new GraphMap<string, IGraphNode<string>, IGraphEdge<string>>()
            {
                new GraphNode<string>("Node1"),
                new GraphNode<string>("Node2"),
                new GraphEdge<string>("Node1", "Node2"),
            };

            map.Nodes.Count.Should().Be(2);
            map.Edges.Count.Should().Be(1);
        }

        [Fact]
        public void EdgeSameNodeTest()
        {
            GraphMap<string, IGraphNode<string>, IGraphEdge<string>> map = null!;

            Action test = () =>
            {
                map = new GraphMap<string, IGraphNode<string>, IGraphEdge<string>>()
                {
                    new GraphNode<string>("Node1"),
                    new GraphNode<string>("Node2"),
                    new GraphEdge<string>("Node1", "Node1"),
                };
            };

            test.Should().Throw<ArgumentException>();
            map.Should().BeNull();
        }
    }
}
