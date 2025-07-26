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

    internal GraphNodeIndex(GraphMap map, object syncLock, GraphMapCounter mapCounters)
    {
        _map = map.NotNull();
        _lock = syncLock.NotNull();
        _index = new ConcurrentDictionary<string, GraphNode>(StringComparer.OrdinalIgnoreCase);
        _tagIndex = new TagIndex<string>(StringComparer.OrdinalIgnoreCase);
        _uniqueIndex = new GraphUniqueIndex(_lock);

        NodeCounter = mapCounters.Nodes;
    }

    public GraphNode this[string key]
    {
        get => _index[key].Action(_ => NodeCounter?.IndexHit.Add());
        internal set => Set(value).ThrowOnError();
    }

    public int Count => _index.Count;
    public bool ContainsKey(string key) => _index.ContainsKey(key);
    public bool TryGetValue(string key, [NotNullWhen(true)] out GraphNode? value) => _index.TryGetValue(key, out value).Action(x => NodeCounter?.Index(x));
    public IReadOnlyList<string> LookupTag(string tag) => _tagIndex.Lookup(tag).Action(x => NodeCounter?.Index(x.Count > 0));
    public Option<UniqueIndex> LookupIndex(string indexName, string indexValue) => _uniqueIndex.Lookup(indexName, indexValue).Action(x => NodeCounter?.Index(x));
    public IReadOnlyList<UniqueIndex> LookupByNodeKey(string nodeKey) => _uniqueIndex.LookupByNodeKey(nodeKey).Action(x => NodeCounter?.Index(x.Count > 0));
    internal NodeCounter NodeCounter { get; }

    public Option Add(GraphNode node, GraphTrxContext? trxContext = null)
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
            NodeCounter.Count.Record(_index.Count);

            _tagIndex.Set(node.Key, node.Tags);
            _uniqueIndex.Set(node, null, trxContext).ThrowOnError();

            NodeCounter.Added.Add();
            trxContext?.TransactionScope.NodeAdd(newNode);
            return StatusCode.OK;
        }
    }

    public IReadOnlyList<GraphNode> LookupTaggedNodes(string tag)
    {
        lock (_lock)
        {
            var nodes = _tagIndex.Lookup(tag);
            var list = nodes.Select(x => _index[x]).ToImmutableArray();
            NodeCounter.Index(list.Length > 0);
            return list;
        }
    }

    internal Option Remove(string key, GraphTrxContext? trxContext = null)
    {
        lock (_lock)
        {
            if (!_index.Remove(key, out var oldValue)) return StatusCode.NotFound;

            _tagIndex.Remove(key);
            _uniqueIndex.RemoveNodeKey(key, trxContext);
            removedNodeFromEdges(oldValue!, trxContext);

            trxContext?.TransactionScope.NodeDelete(oldValue);
            NodeCounter?.Deleted.Add();
            NodeCounter?.Count.Record(_index.Count);
            return StatusCode.OK;
        }

        void removedNodeFromEdges(GraphNode graphNode, GraphTrxContext? graphContext)
        {
            var edges = _map.Edges.LookupByNodeKey([graphNode.Key]);
            edges.ForEach(x => _map.Edges.Remove(x, graphContext));
        }
    }

    internal Option Set(GraphNode node, GraphTrxContext? trxContext = null)
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

            trxContext?.Action(x =>
            {
                if (exist)
                    x.TransactionScope.NodeChange(current.NotNull(), updatedNode);
                else
                    x.TransactionScope.NodeAdd(updatedNode);
            });


            if (exist) NodeCounter?.Updated.Add(); else NodeCounter?.Added.Add();
            NodeCounter?.Count.Record(_index.Count);
            return StatusCode.OK;
        }

        GraphNode build(IReadOnlyDictionary<string, string?> tags, IReadOnlyCollection<string> indexes, IReadOnlyDictionary<string, string?> foreignKeys) =>
            new GraphNode(node.Key, tags, node.CreatedDate, node.DataMap, indexes, foreignKeys);
    }

    public IEnumerator<GraphNode> GetEnumerator() => _index.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal void UpdateCounters() => NodeCounter?.Count.Record(_index.Count);
}
