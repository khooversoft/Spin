using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

[assembly: InternalsVisibleTo("Toolbox.Graph.test")]

namespace Toolbox.Graph;

public class GraphNodeIndex : IEnumerable<GraphNode>
{
    private readonly ConcurrentDictionary<string, GraphNode> _index;
    private readonly TagIndex<string> _tagIndex;
    private readonly GraphUniqueIndex _uniqueIndex;
    private readonly object _lock;
    private readonly GraphMap _map;

    internal GraphNodeIndex(GraphMap map, object syncLock)
    {
        _map = map.NotNull();
        _lock = syncLock.NotNull();
        _index = new ConcurrentDictionary<string, GraphNode>(StringComparer.OrdinalIgnoreCase);
        _tagIndex = new TagIndex<string>(StringComparer.OrdinalIgnoreCase);
        _uniqueIndex = new GraphUniqueIndex(_lock);
    }

    public GraphNode this[string key]
    {
        get => _index[key].Action(_ => _map.Meter.Node.IndexHit());
        internal set => Set(value).ThrowOnError();
    }

    public int Count => _index.Count;
    public bool ContainsKey(string key) => _index.ContainsKey(key);
    public bool TryGetValue(string key, [NotNullWhen(true)] out GraphNode? value) => _index.TryGetValue(key, out value).Action(x => _map.Meter.Node.Index(x));
    public IReadOnlyList<string> LookupTag(string tag) => _tagIndex.Lookup(tag).Action(x => _map.Meter.Node.Index(x.Count > 0));
    public Option<UniqueIndex> LookupIndex(string indexName, string indexValue) => _uniqueIndex.Lookup(indexName, indexValue).Action(x => _map.Meter.Node.Index(x));
    public IReadOnlyList<UniqueIndex> LookupByNodeKey(string nodeKey) => _uniqueIndex.LookupByNodeKey(nodeKey).Action(x => _map.Meter.Node.Index(x.Count > 0));

    internal Option Add(GraphNode node, IGraphTrxContext? trxContext = null)
    {
        if (node.Validate().IsError(out var v)) return v;

        lock (_lock)
        {
            var activeIndexesOption = _uniqueIndex.Verify(node, null, trxContext);
            if (activeIndexesOption.IsError()) return (StatusCode.Conflict, $"Node key={node.Key} already exist");

            var newNode = new GraphNode(
                node.Key,
                node.Tags.RemoveDeleteCommands(),
                node.CreatedDate,
                node.DataMap,
                node.Indexes.RemoveDeleteCommands(),
                node.ForeignKeys.RemoveDeleteCommands()
                );

            if (!_index.TryAdd(newNode.Key, newNode)) return (StatusCode.Conflict, $"Node key={node.Key} already exist");

            _tagIndex.Set(node.Key, node.Tags);
            _uniqueIndex.Set(node, null, trxContext).ThrowOnError();

            _map.Meter.Node.Added();
            trxContext?.ChangeLog.Push(new CmNodeAdd(node));
            return StatusCode.OK;
        }
    }

    internal void Clear()
    {
        lock (_lock)
        {
            _index.Clear();
            _tagIndex.Clear();
            _uniqueIndex.Clear();
        }
    }

    public IReadOnlyList<GraphNode> LookupTaggedNodes(string tag)
    {
        lock (_lock)
        {
            var nodes = _tagIndex.Lookup(tag);
            var list = nodes.Select(x => _index[x]).ToImmutableArray();
            _map.Meter.Node.Index(list.Length > 0);
            return list;
        }
    }

    internal Option Remove(string key, IGraphTrxContext? trxContext = null)
    {
        lock (_lock)
        {
            if (!_index.Remove(key, out var oldValue)) return StatusCode.NotFound;

            _tagIndex.Remove(key);
            _uniqueIndex.RemoveNodeKey(key, trxContext);
            removedNodeFromEdges(oldValue!, trxContext);

            trxContext?.ChangeLog.Push(new CmNodeDelete(oldValue));
            _map.Meter.Node.Deleted();
            return StatusCode.OK;
        }

        void removedNodeFromEdges(GraphNode graphNode, IGraphTrxContext? graphContext)
        {
            var edges = _map.Edges.LookupByNodeKey([graphNode.Key]);
            edges.ForEach(x => _map.Edges.Remove(x, graphContext));
        }
    }

    internal Option Set(GraphNode node, IGraphTrxContext? trxContext = null)
    {
        if (node.Validate().IsError(out var v)) return v;

        lock (_lock)
        {
            var activeIndexesOption = _uniqueIndex.Verify(node, null, trxContext);
            if (activeIndexesOption.IsError()) return (StatusCode.Conflict, $"Node key={node.Key} already exist");

            (bool exist, GraphNode updatedNode) = _index.TryGetValue(node.Key, out GraphNode? current) switch
            {
                false => (false, build(node.Tags.RemoveDeleteCommands(), node.Indexes.RemoveDeleteCommands(), node.ForeignKeys.RemoveDeleteCommands())),
                true => (true, build(node.Tags.MergeAndFilter(current.Tags), node.Indexes.MergeCommands(current.Indexes), node.ForeignKeys.MergeAndFilter(current.ForeignKeys))),
            };

            _index[node.Key] = updatedNode;
            _tagIndex.Set(node.Key, node.Tags);
            _uniqueIndex.Set(node, current, trxContext).ThrowOnError();

            trxContext?.ChangeLog.Push(exist switch
            {
                false => new CmNodeAdd(updatedNode),
                true => new CmNodeChange(current!, updatedNode),
            });

            if (exist) _map.Meter.Node.Updated(); else _map.Meter.Node.Added();
            return StatusCode.OK;
        }

        GraphNode build(IReadOnlyDictionary<string, string?> tags, IReadOnlyCollection<string> indexes, IReadOnlyDictionary<string, string?> foreignKeys) =>
            new GraphNode(node.Key, tags, node.CreatedDate, node.DataMap, indexes, foreignKeys);
    }

    public IEnumerator<GraphNode> GetEnumerator() => _index.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
