using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

/// <summary>
/// Index to lookup unique keys associagted with an index (tag key) + tagValue + nodekey
/// 
/// Lookup -
///   tagKey + tagValue -> nodeKeys
///   
/// </summary>
public class GraphUniqueIndex
{
    private readonly object _syncLock;

    private readonly TagValueIndex _tagIndex = new TagValueIndex(); // Master index

    public GraphUniqueIndex(object syncLock) => _syncLock = syncLock.NotNull();

    public Option Set(GraphNode newNode, GraphNode? currentNode, IGraphTrxContext? trxContext)
    {
        newNode.NotNull();

        lock (_syncLock)
        {
            var verifyOption = Verify(newNode, currentNode, trxContext);
            if (verifyOption.IsError()) return verifyOption;

            AddToIndex(newNode, currentNode, trxContext);
            return StatusCode.OK;
        }
    }

    public void Clear() => _tagIndex.Clear();
    public Option<UniqueIndex> Lookup(string indexName, string value) => _tagIndex.Lookup(indexName, value);
    public IReadOnlyList<UniqueIndex> LookupByNodeKey(string nodeKey) => _tagIndex.LookupByNodeKey(nodeKey);

    public Option Verify(GraphNode newNode, GraphNode? currentNode, IGraphTrxContext? trxContext)
    {
        newNode.NotNull();

        if (currentNode != null && !newNode.Key.EqualsIgnoreCase(currentNode.Key)) return (StatusCode.BadRequest, "Node keys do not match");

        // Verify that unique contraints are not violated
        lock (_syncLock)
        {
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
    }

    public void RemoveNodeKey(string nodeKey, IGraphTrxContext? trxContext)
    {
        lock (_syncLock)
        {
            var status = _tagIndex.RemoveNodeKey(nodeKey);

            trxContext?.Context.Log(
                status ? LogLevel.Information : LogLevel.Warning,
                status ? "Removed" : "Failed to remove" + " nodeKey={nodeKey}", nodeKey
                );
        }
    }

    private void AddToIndex(GraphNode newNode, GraphNode? currentNode, IGraphTrxContext? trxContext)
    {
        var indexedTags = GetIndexedTags(newNode, currentNode);

        // Remove node indexes
        var status = _tagIndex.RemoveNodeKey(newNode.Key);

        indexedTags.ForEach(x =>
        {
            var option = _tagIndex.Add(x.Key, x.Value, newNode.Key);

            trxContext?.Context.Log(
                option.IsOk() ? LogLevel.Information : LogLevel.Warning,
                option.IsOk() ? "Added" : "Failed to add" + " key={key}={value} for nodeKey={nodeKey}", x.Key, x.Value, newNode.Key
                );
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
        var deleteIndexCommands = UniqueIndexTool.GetDeleteCommands(newNode.Indexes);

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