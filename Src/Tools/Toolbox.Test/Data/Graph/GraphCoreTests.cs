using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Data.Graph;

public class GraphCoreTests
{
    [Fact]
    public void SingleNode()
    {
        var graph = new GraphCore();

        var node = new Node("A", "Node A".ToDataETag());
        var badNode = new Node("A", "Node B".ToDataETag());

        graph.Nodes.TryAdd(node).ThrowOnError();

        graph.Nodes.TryGetValue("A", out var value).Action(x =>
        {
            x.BeTrue();
            node.Be(node);
            Verify.Throws<ArgumentException>(() => node.Be(badNode));
        });
    }

    [Fact]
    public void AddDuplicateNodeShouldReturnConflict()
    {
        var graph = new GraphCore();

        graph.Nodes.TryAdd(new Node("A")).ThrowOnError();

        var result = graph.Nodes.TryAdd(new Node("A", "payload".ToDataETag()));

        result.BeConflict();
    }

    [Fact]
    public void EdgeRequiresExistingNodes()
    {
        var graph = new GraphCore();

        graph.Nodes.TryAdd(new Node("A")).ThrowOnError();

        var result = graph.Edges.TryAdd(new Edge("A", "B", "type1"));
        result.BeNotFound();

        graph.Edges.Contains(Edge.CreateKey("A", "B", "type1")).BeFalse();
    }

    [Fact]
    public void AddEdgeAndLookup()
    {
        var graph = new GraphCore();

        graph.Nodes.TryAdd(new Node("A")).ThrowOnError();
        graph.Nodes.TryAdd(new Node("B")).ThrowOnError();

        var edge = new Edge("A", "B", "type1", "payload".ToDataETag());
        graph.Edges.TryAdd(edge).ThrowOnError();

        graph.Edges.TryGetValue(edge.EdgeKey, out var byKey).BeTrue();
        (edge == byKey).BeTrue();

        graph.Edges.TryGetValue("A", "B", "type1", out var byParts).BeTrue();
        (edge == byParts).BeTrue();

        graph.Edges.GetByFrom("A").Contains(edge).BeTrue();
        graph.Edges.GetByTo("B").Contains(edge).BeTrue();
        graph.Edges.GetByType("type1").Contains(edge).BeTrue();
    }

    [Fact]
    public void RemovingNodeShouldRemoveEdges()
    {
        var graph = new GraphCore();

        graph.Nodes.TryAdd(new Node("A")).ThrowOnError();
        graph.Nodes.TryAdd(new Node("B")).ThrowOnError();
        graph.Nodes.TryAdd(new Node("C")).ThrowOnError();

        var edge1 = new Edge("A", "B", "type1");
        var edge2 = new Edge("C", "A", "type2");

        graph.Edges.TryAdd(edge1).ThrowOnError();
        graph.Edges.TryAdd(edge2).ThrowOnError();

        graph.Nodes.Remove("A").ThrowOnError();

        graph.Nodes.TryGetValue("A", out _).BeFalse();
        graph.Edges.Contains(edge1.EdgeKey).BeFalse();
        graph.Edges.Contains(edge2.EdgeKey).BeFalse();
        graph.Edges.GetByFrom("A").Any().BeFalse();
        graph.Edges.GetByTo("A").Any().BeFalse();
    }

    [Fact]
    public void DuplicateEdgeShouldReturnConflict()
    {
        var graph = new GraphCore();

        graph.Nodes.TryAdd(new Node("A")).ThrowOnError();
        graph.Nodes.TryAdd(new Node("B")).ThrowOnError();

        graph.Edges.TryAdd(new Edge("A", "B", "type1")).ThrowOnError();

        var result = graph.Edges.TryAdd(new Edge("A", "B", "type1"));

        result.Be(StatusCode.Conflict);
    }

    [Fact]
    public void EdgeCannotReferenceSameNode()
    {
        Verify.Throws<ArgumentException>(() => new Edge("A", "A", "type1"));
    }
}
