using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Data;

public partial class GraphCore : IEquatable<GraphCore>
{
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);
    private readonly DgInternalCalls _calls = new();

    public GraphCore()
    {
        _calls.GetRecorder = () => _recorder;

        Nodes = new NodeIndex(Array.Empty<Node>(), _calls, _lock);
        Edges = new EdgeIndex(Array.Empty<Edge>(), _calls, _lock);
    }

    public GraphCore(IEnumerable<Node> nodes, IEnumerable<Edge> edges)
    {
        nodes.NotNull();
        edges.NotNull();

        _calls.GetRecorder = () => _recorder;

        Nodes = new NodeIndex(nodes, _calls, _lock);
        Edges = new EdgeIndex(edges, _calls, _lock);
    }

    public GraphCore(GraphCoreSerialization graphCoreSerialization)
        : this(graphCoreSerialization.Nodes, graphCoreSerialization.Edges)
    {
    }

    public NodeIndex Nodes { get; }
    public EdgeIndex Edges { get; }

    /// <summary>
    /// Clear node and edge indexes, no changes are logged
    /// </summary>
    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            _calls.ClearNodes();
            _calls.ClearEdges();
            _logSequenceNumber = null;
        }
        finally { _lock.ExitWriteLock(); }
    }

    public string ToJson() => this.ToSerialization().ToJson();

    public bool Equals(GraphCore? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        var thisNodes = Nodes.OrderBy(x => x.NodeKey, StringComparer.OrdinalIgnoreCase).ToArray();
        var otherNodes = other.Nodes.OrderBy(x => x.NodeKey, StringComparer.OrdinalIgnoreCase).ToArray();
        if (!thisNodes.SequenceEqual(otherNodes)) return false;

        var thisEdges = Edges.OrderBy(x => x.EdgeKey, StringComparer.OrdinalIgnoreCase).ToArray();
        var otherEdges = other.Edges.OrderBy(x => x.EdgeKey, StringComparer.OrdinalIgnoreCase).ToArray();

        return thisEdges.SequenceEqual(otherEdges);
    }

    public override bool Equals(object? obj) => Equals(obj as GraphCore);

    public override int GetHashCode()
    {
        HashCode hash = new();

        foreach (var node in Nodes.OrderBy(x => x.NodeKey, StringComparer.OrdinalIgnoreCase)) hash.Add(node);
        foreach (var edge in Edges.OrderBy(x => x.EdgeKey, StringComparer.OrdinalIgnoreCase)) hash.Add(edge);

        return hash.ToHashCode();
    }

    public static bool operator ==(GraphCore? left, GraphCore? right) => EqualityComparer<GraphCore>.Default.Equals(left, right);
    public static bool operator !=(GraphCore? left, GraphCore? right) => !(left == right);
}
