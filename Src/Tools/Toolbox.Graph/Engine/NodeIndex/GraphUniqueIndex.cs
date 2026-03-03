using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

/// <summary>
/// Index to lookup unique keys associated with an index (tag key) + tagValue + nodeKey
/// 
/// Lookup -
///   tagKey + tagValue -> nodeKeys
///   
/// </summary>
internal class GraphUniqueIndex
{
    private readonly ReaderWriterLockSlim _gate = new();

    private readonly TagValueIndex _tagIndex = new TagValueIndex(); // Master index
    private readonly ILogger _logger;

    public GraphUniqueIndex(ILogger logger)
    {
        _logger = logger.NotNull();
    }


    public void Clear() => _tagIndex.Clear();
    public Option<UniqueIndex> Lookup(string indexName, string value)
    {
        _gate.EnterReadLock();
        try
        {
            return _tagIndex.Lookup(indexName, value);
        }
        finally { _gate.ExitReadLock(); }
    }

    public IReadOnlyList<UniqueIndex> LookupByNodeKey(string nodeKey)
    {
        _gate.EnterReadLock();
        try
        {
            return _tagIndex.LookupByNodeKey(nodeKey);
        }
        finally { _gate.ExitReadLock(); }
    }

    internal Option InternalSet(GraphNode newNode, GraphNode? currentNode, GraphTrxContext? trxContext)
    {
        newNode.NotNull();

        _gate.EnterWriteLock();
        try
        {
            var verifyOption = InternalVerify(newNode, currentNode, trxContext);
            if (verifyOption.IsError()) return verifyOption;

            InternalAddToIndex(newNode, currentNode, trxContext);
            return StatusCode.OK;
        }
        finally { _gate.ExitWriteLock(); }
    }

    public void RemoveNodeKey(string nodeKey, GraphTrxContext? trxContext)
    {
        _gate.EnterWriteLock();
        try
        {
            Option status = _tagIndex.RemoveNodeKey(nodeKey);
            _logger.LogStatus(status, "nodeKey={nodeKey}", [nodeKey]);
        }
        finally { _gate.ExitWriteLock(); }
    }

    public Option Verify(GraphNode newNode, GraphNode? currentNode, GraphTrxContext? trxContext)
    {
        _gate.EnterReadLock();
        try
        {
            return InternalVerify(newNode, currentNode, trxContext);
        }
        finally { _gate.ExitReadLock(); }
    }

    private Option InternalVerify(GraphNode newNode, GraphNode? currentNode, GraphTrxContext? trxContext)
    {
        newNode.NotNull();
        if (currentNode != null && !newNode.Key.EqualsIgnoreCase(currentNode.Key)) return (StatusCode.BadRequest, "Node keys do not match");

        // Node cannot have an indexed value that another node has
        var indexedTags = GetIndexedTags(newNode, currentNode);

        var lookupResult = indexedTags
            .Select(x => _tagIndex.Lookup(x.Key, x.Value))
            .Where(x => x.IsOk())
            .Select(x => x.Return())
            .ToArray();

        var indexedTagsByOtherNodes = lookupResult
            .Where(x => !x.NodeKey.EqualsIgnoreCase(newNode.Key))
            .ToArray();

        if (indexedTagsByOtherNodes.Length != 0)
        {
            string msg = indexedTagsByOtherNodes.Select(x => $"NodeKey={newNode.Key}, {x.IndexName}={x.Value}").Join(",");
            return (StatusCode.BadRequest, msg);
        }

        return StatusCode.OK;
    }

    private void InternalAddToIndex(GraphNode newNode, GraphNode? currentNode, GraphTrxContext? trxContext)
    {
        var indexedTags = GetIndexedTags(newNode, currentNode);

        // Remove node indexes
        _tagIndex.RemoveNodeKey(newNode.Key);

        indexedTags.ForEach(x =>
        {
            var option = _tagIndex.Add(x.Key, x.Value, newNode.Key);
            _logger.LogStatus(option, "Add index={index}={value} for nodeKey={nodeKey}", [x.Key, x.Value, newNode.Key]);
        });
    }

    private static IReadOnlyList<KeyValuePair<string, string>> GetIndexedTags(GraphNode newNode, GraphNode? currentNode) => GetActiveTags(newNode, currentNode)
        .Join(GetActiveIndexes(newNode, currentNode), x => x.Key, y => y, (tag, index) => tag, StringComparer.OrdinalIgnoreCase)
        .Distinct(TagsComparer.Default)
        .ToArray();

    private static IReadOnlyList<string> GetActiveIndexes(GraphNode newNode, GraphNode? currentNode) => newNode.Indexes
        .Concat(currentNode?.Indexes ?? Array.Empty<string>())
        .Where(x => !TagsTool.HasRemoveFlag(x))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    private static IReadOnlyList<KeyValuePair<string, string>> GetActiveTags(GraphNode newNode, GraphNode? currentNode)
    {
        newNode.NotNull();

        var deleteTabCommands = TagsTool.GetTagDeleteCommands(newNode.Tags);
        var deleteIndexCommands = GraphTool.GetDeleteCommands(newNode.Indexes);

        var list = GetTagPairs(newNode.NotNull().Tags)
            .Concat(currentNode != null ? GetTagPairs(currentNode.Tags) : Array.Empty<KeyValuePair<string, string>>())
            .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .Distinct(TagsComparer.Default)
            .Where(x => !isDeleted(x.Key))
            .ToArray();

        return list;

        bool isDeleted(string value) =>
            deleteTabCommands.Contains(value, StringComparer.OrdinalIgnoreCase) ||
            deleteIndexCommands.Contains(value, StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<KeyValuePair<string, string>> GetTagPairs(IEnumerable<KeyValuePair<string, string?>> tags) => tags.NotNull()
        .Where(x => !TagsTool.HasRemoveFlag(x.Key))
        .Where(x => x.Value.IsNotEmpty())
        .Select(x => new KeyValuePair<string, string>(x.Key, x.Value.NotEmpty()));
}