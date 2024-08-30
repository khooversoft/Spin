using System.Collections.Immutable;
using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal class InterExecutionContext
{
    public InterExecutionContext(IEnumerable<IGraphInstruction> graphInstructions)
    {
        Instructions = graphInstructions.NotNull().ToList();
        Cursor = new Cursor<IGraphInstruction>(Instructions);
    }

    public List<IGraphInstruction> Instructions { get; init; }
    public Cursor<IGraphInstruction> Cursor { get; init; }
    public DictionaryList<string, WorkingDataSet> DataSets { get; init; } = new DictionaryList<string, WorkingDataSet>(x => x.Alias);
    public DictionaryList<string, GraphLink> Links { get; init; } = new DictionaryList<string, GraphLink>(x => x.NodeKey);
}


internal class WorkingDataSet
{
    public WorkingDataSet(IEnumerable<GraphNode> graphNodes) => (IsNode, Nodes) = (true, graphNodes.NotNull().ToImmutableArray());
    public WorkingDataSet(string alias, IEnumerable<GraphNode> graphNodes) => (IsNode, Alias, Nodes) = (true, alias.NotEmpty(), graphNodes.NotNull().ToImmutableArray());

    public WorkingDataSet(IEnumerable<GraphEdge> graphEdges) => Edges = graphEdges.NotNull().ToImmutableArray();
    public WorkingDataSet(string alias, IEnumerable<GraphEdge> graphEdges) => (Alias, Edges) = (alias, graphEdges.NotNull().ToImmutableArray());

    public bool IsNode { get; }
    public bool IsEdge => !IsNode;
    public string Alias { get; } = Guid.NewGuid().ToString();
    public IReadOnlyList<GraphNode> Nodes { get; } = Array.Empty<GraphNode>();
    public IReadOnlyList<GraphEdge> Edges { get; } = Array.Empty<GraphEdge>();
}
