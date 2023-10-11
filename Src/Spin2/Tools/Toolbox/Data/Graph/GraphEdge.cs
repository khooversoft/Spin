using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public interface IGraphEdge<TKey> : IGraphCommon
{
    Guid Key { get; }
    TKey FromNodeKey { get; }
    TKey ToNodeKey { get; }
    IReadOnlyDictionary<string, string?> Tags { get; }
}

public record GraphEdge<TKey> : IGraphEdge<TKey>
{
    private static IReadOnlyDictionary<string, string?> _default = new Dictionary<string, string?>();

    public GraphEdge(TKey fromNodeKey, TKey toNodeKey, string? tags = null)
    {
        FromNodeKey = fromNodeKey.NotNull();
        ToNodeKey = toNodeKey.NotNull();
        Tags = tags != null ? new Tags().Set(tags) : _default;
    }

    public Guid Key { get; } = Guid.NewGuid();
    public TKey FromNodeKey { get; init; }
    public TKey ToNodeKey { get; init; }
    public IReadOnlyDictionary<string, string?> Tags { get; init; } = _default;

}



//public class GraphEdgeIndex<TKey> : Dictionary<TKey, HashSet<Guid>>
//    where TKey : notnull
//{
//    private readonly Dictionary<Guid, HashSet<TKey>> _reverseLookup;

//    public GraphEdgeIndex(IEqualityComparer<TKey>? equalityComparer = null)
//        : base(equalityComparer.ComparerFor())
//    {
//        _reverseLookup = new Dictionary<Guid, HashSet<TKey>>();
//    }

//    public new HashSet<Guid> this[TKey key]
//    {
//        get => base[key];
//        set
//        {
//            var hashset = base[key];
//        }
//    }

//    public void Add(TKey key, HashSet<Guid> value)
//    {
//        throw new NotImplementedException();
//    }

//    public void Add(KeyValuePair<TKey, HashSet<Guid>> item)
//    {
//        throw new NotImplementedException();
//    }

//    public void Clear()
//    {
//        throw new NotImplementedException();
//    }

//    public bool Remove(TKey key)
//    {
//        throw new NotImplementedException();
//    }

//    public bool Remove(KeyValuePair<TKey, HashSet<Guid>> item)
//    {
//        throw new NotImplementedException();
//    }
//}
