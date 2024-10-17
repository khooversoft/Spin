﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

[assembly: InternalsVisibleTo("Toolbox.Graph.test")]

namespace Toolbox.Graph;

public class GraphNodeIndex : IEnumerable<GraphNode>
{
    private readonly Dictionary<string, GraphNode> _index;
    private readonly TagIndex<string> _tagIndex;
    private readonly object _lock;
    private readonly GraphRI _graphRI;
    private readonly GraphUniqueIndex _uniqueIndex;

    internal GraphNodeIndex(object syncLock, GraphRI graphRI)
    {
        _lock = syncLock.NotNull();
        _graphRI = graphRI.NotNull();
        _index = new Dictionary<string, GraphNode>(StringComparer.OrdinalIgnoreCase);
        _tagIndex = new TagIndex<string>(StringComparer.OrdinalIgnoreCase);
        _uniqueIndex = new GraphUniqueIndex(_lock);
    }

    public GraphNode this[string key]
    {
        get => _index[key];
        internal set => Set(value).ThrowOnError();
    }

    public int Count => _index.Count;
    public bool ContainsKey(string key) => _index.ContainsKey(key);
    public bool TryGetValue(string key, [NotNullWhen(true)] out GraphNode? value) => _index.TryGetValue(key, out value);
    public IReadOnlyList<string> LookupTag(string tag) => _tagIndex.Lookup(tag);
    public Option<UniqueIndex> LookupIndex(string indexName, string indexValue) => _uniqueIndex.Lookup(indexName, indexValue);
    public IReadOnlyList<UniqueIndex> LookupByNodeKey(string nodeKey) => _uniqueIndex.LookupByNodeKey(nodeKey);

    internal Option Add(GraphNode node, IGraphTrxContext? trxContext = null)
    {
        if (!node.Validate(out var v)) return v;

        lock (_lock)
        {
            var activeIndexesOption = _uniqueIndex.Verify(node, null, trxContext);
            if (activeIndexesOption.IsError()) return (StatusCode.Conflict, $"Node key={node.Key} already exist");

            var newNode = new GraphNode(node.Key, node.Tags.RemoveDeleteCommands(), node.CreatedDate, node.DataMap, node.Indexes.RemoveDeleteCommands());
            if (!_index.TryAdd(newNode.Key, newNode)) return (StatusCode.Conflict, $"Node key={node.Key} already exist");

            _tagIndex.Set(node.Key, node.Tags);
            _uniqueIndex.Set(node, null, trxContext).ThrowOnError();

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
        }
    }

    public IReadOnlyList<GraphNode> LookupTaggedNodes(string tag)
    {
        lock (_lock)
        {
            var nodes = _tagIndex.Lookup(tag);
            var list = nodes.Select(x => _index[x]).ToArray();
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
            _graphRI.RemovedNodeFromEdges(oldValue!, trxContext);

            trxContext?.ChangeLog.Push(new CmNodeDelete(oldValue));
            return StatusCode.OK;
        }
    }

    internal Option Set(GraphNode node, IGraphTrxContext? trxContext = null)
    {
        if (!node.Validate(out var v)) return v;

        lock (_lock)
        {
            var activeIndexesOption = _uniqueIndex.Verify(node, null, trxContext);
            if (activeIndexesOption.IsError()) return (StatusCode.Conflict, $"Node key={node.Key} already exist");

            (bool exist, GraphNode updatedNode) = _index.TryGetValue(node.Key, out GraphNode? current) switch
            {
                false => (false, new GraphNode(node.Key, node.Tags.RemoveDeleteCommands(), node.CreatedDate, node.DataMap, node.Indexes.RemoveDeleteCommands())),
                //true => (true, new GraphNode(node.Key, node.Tags.RemoveDeleteCommands(), current.CreatedDate, node.DataMap, node.Indexes.MergeIndexes(current.Indexes))),
                true => (true, new GraphNode(node.Key, node.Tags.MergeAndFilter(current.Tags), current.CreatedDate, node.DataMap, node.Indexes.MergeIndexes(current.Indexes))),
            };

            _index[node.Key] = updatedNode;
            _tagIndex.Set(node.Key, node.Tags);
            _uniqueIndex.Set(node, current, trxContext).ThrowOnError();

            trxContext?.ChangeLog.Push(exist switch
            {
                false => new CmNodeAdd(updatedNode),
                true => new CmNodeChange(current!, updatedNode),
            });

            return StatusCode.OK;
        }
    }

    public IEnumerator<GraphNode> GetEnumerator() => _index.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
