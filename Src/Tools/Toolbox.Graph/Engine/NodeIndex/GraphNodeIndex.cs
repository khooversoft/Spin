using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Toolbox.Extensions;
using Toolbox.Telemetry;
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
    private readonly ITelemetryCounter<long>? _addedCounter;
    private readonly ITelemetryCounter<long>? _deletedCounter;
    private readonly ITelemetryCounter<long>? _updatedCounter;
    private readonly ITelemetryCounter<long>? _indexScanCounter;
    private readonly ITelemetryRecorder<long>? _countGauge;

    internal GraphNodeIndex(GraphMap map, object syncLock, ITelemetry? telemetry = null)
    {
        _map = map.NotNull();
        _lock = syncLock.NotNull();

        _index = new ConcurrentDictionary<string, GraphNode>(StringComparer.OrdinalIgnoreCase);
        _tagIndex = new TagIndex<string>(StringComparer.OrdinalIgnoreCase);
        _uniqueIndex = new GraphUniqueIndex(_lock);

        _addedCounter = telemetry?.CreateCounter<long>("graph.node.add", "Number of nodes added", unit: "count");
        _deletedCounter = telemetry?.CreateCounter<long>("graph.node.delete", "Number of nodes deleted", unit: "count");
        _updatedCounter = telemetry?.CreateCounter<long>("graph.node.update", "Number of nodes updated", unit: "count");
        _indexScanCounter = telemetry?.CreateCounter<long>("graph.node.index.scan", "Number of index scans", unit: "count");
        _countGauge = telemetry?.CreateGauge<long>("graph.node.count", "Current node count", unit: "count");
    }

    public GraphNode this[string key]
    {
        get => _index[key];
        internal set => Set(value).ThrowOnError();
    }

    public int Count => _index.Count;
    public void AddIndexScan(int count = 1) => _indexScanCounter?.Add(count);
    public bool ContainsKey(string key) => _index.ContainsKey(key);

    public bool TryGetValue(string key, [NotNullWhen(true)] out GraphNode? value) => _index.TryGetValue(key, out value);

    public IReadOnlyList<string> LookupTag(string tag) => _tagIndex.Lookup(tag);

    public Option<UniqueIndex> LookupIndex(string indexName, string indexValue) => _uniqueIndex.Lookup(indexName, indexValue);

    public IReadOnlyList<UniqueIndex> LookupByNodeKey(string nodeKey) => _uniqueIndex.LookupByNodeKey(nodeKey);

    public Option Add(GraphNode node, GraphTrxContext? trxContext = null)
    {
        if (node.Validate().IsError(out var v)) return v;

        lock (_lock)
        {
            Option activeIndexesOption = _uniqueIndex.Verify(node, null, trxContext);
            if (activeIndexesOption.IsError()) return (StatusCode.Conflict, $"Node key={node.Key} already exist");

            var newNode = new GraphNode(
                node.Key,
                node.Tags.RemoveDeleteCommands(),
                node.CreatedDate,
                node.DataMap,
                node.Indexes.RemoveDeleteCommands(),
                node.ForeignKeys.RemoveDeleteCommands(),
                node.Grants);

            if (!_index.TryAdd(newNode.Key, newNode)) return (StatusCode.Conflict, $"Node key={node.Key} already exist");
            GaugePostRecordCount();

            _tagIndex.Set(node.Key, node.Tags);
            _uniqueIndex.Set(node, null, trxContext).ThrowOnError();

            _addedCounter?.Increment();
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
            return list;
        }
    }

    internal Option Remove(string key, GraphTrxContext? trxContext = null)
    {
        lock (_lock)
        {
            if (!_index.Remove(key, out GraphNode? oldValue)) return StatusCode.NotFound;

            _tagIndex.Remove(key);
            _uniqueIndex.RemoveNodeKey(key, trxContext);
            removedNodeFromEdges(oldValue!, trxContext);

            trxContext?.TransactionScope.NodeDelete(oldValue);
            _deletedCounter?.Increment();
            GaugePostRecordCount();
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
            Option activeIndexesOption = _uniqueIndex.Verify(node, null, trxContext);
            if (activeIndexesOption.IsError()) return (StatusCode.Conflict, $"Node key={node.Key} already exist");

            (bool exist, GraphNode updatedNode) = _index.TryGetValue(node.Key, out GraphNode? current) switch
            {
                false => (false, build(node.Tags.RemoveDeleteCommands(), node.Indexes.RemoveDeleteCommands(), node.ForeignKeys.RemoveDeleteCommands(), node.Grants)),
                true => (true, build(node.Tags.MergeAndFilter(current.Tags), node.Indexes.MergeCommands(current.Indexes), node.ForeignKeys.MergeAndFilter(current.ForeignKeys), node.Grants)),
            };

            _index[node.Key] = updatedNode;
            _tagIndex.Set(node.Key, node.Tags);
            _uniqueIndex.Set(node, current, trxContext).ThrowOnError();

            trxContext?.Action(x =>
            {
                if (exist)
                {
                    x.TransactionScope.NodeChange(current.NotNull(), updatedNode);
                }
                else
                {
                    x.TransactionScope.NodeAdd(updatedNode);
                }
            });

            if (exist) _updatedCounter?.Increment(); else _addedCounter?.Increment();

            GaugePostRecordCount();
            return StatusCode.OK;
        }

        GraphNode build(IReadOnlyDictionary<string, string?> tags, IReadOnlyCollection<string> indexes, IReadOnlyDictionary<string, string?> foreignKeys, IReadOnlyCollection<GrantPolicy> policies) =>
            new GraphNode(node.Key, tags, node.CreatedDate, node.DataMap, indexes, foreignKeys, policies);
    }

    public IEnumerator<GraphNode> GetEnumerator() => _index.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void GaugePostRecordCount() => _countGauge?.Post(_index.Count);
}
