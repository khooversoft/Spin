using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphUniqueIndex
{
    private readonly object _syncLock;

    // IndexName (tag name) -> Value (tag value) -> UniqueIndex
    private Dictionary<string, Dictionary<string, UniqueIndex>> _index = new(StringComparer.OrdinalIgnoreCase);
    private NodeKeyLookup _nodeKeyLookup = new();

    public GraphUniqueIndex(object syncLock) => _syncLock = syncLock.NotNull();

    public Option<IReadOnlyList<string>> Set(string nodeKey, IEnumerable<KeyValuePair<string, string?>> tagsWithValue, IEnumerable<string> indexes)
    {
        tagsWithValue.NotNull();
        indexes.NotNull();

        lock (_syncLock)
        {
            var list = GetIndexValues(tagsWithValue, indexes);

            foreach (var item in list)
            {
                var result = item switch
                {
                    (false, var indexName, var tagValue) => Add(new UniqueIndex { IndexName = indexName, Value = tagValue, NodeKey = nodeKey }),
                    (true, var indexName, var tagValue) => Remove(indexName, tagValue),
                };

                if (result.IsError() && !result.IsNotFound()) return result.ToOptionStatus<IReadOnlyList<string>>();
            }

            return list.Where(x => !x.isRemove).Select(x => x.indexName).ToImmutableArray();
        }
    }

    public Option Verify(string nodeKey, IEnumerable<KeyValuePair<string, string?>> tagsWithValue, IEnumerable<string> indexes)
    {
        lock (_syncLock)
        {
            var list = GetIndexValues(tagsWithValue, indexes);

            var errorList = list
                .Where(x => !x.isRemove)
                .Select(x => (x, lookup: Lookup(x.indexName, x.tagValue)))
                .Where(x => x.lookup.IsOk() && x.lookup.Return() != nodeKey)
                .ToArray();

            if (errorList.Length != 0)
            {
                string msg = errorList.Select(x => $"NodeKey={nodeKey}, {x.x.indexName}={x.x.tagValue}").Join(",");
                return (StatusCode.BadRequest, msg);
            }

            return StatusCode.OK;
        }
    }

    public void RemoveNodeKey(string nodeKey)
    {
        lock (_syncLock)
        {
            var list = _nodeKeyLookup.Lookup(nodeKey);
            list.ForEach(x => Remove(x.IndexName, x.Value));
        }
    }

    private Option<string> Lookup(string indexName, string value)
    {
        lock (_syncLock)
        {
            if (!_index.TryGetValue(indexName, out var index)) return new Option<string>();

            if (!index.TryGetValue(value, out var uniqueIndex)) return new Option<string>();

            return uniqueIndex.NodeKey;
        }
    }

    private Option Remove(string indexName, string value)
    {
        lock (_syncLock)
        {
            if (!_index.TryGetValue(indexName, out var index)) return StatusCode.NotFound;

            return index.Remove(value) ? StatusCode.OK : StatusCode.NotFound;
        }
    }

    private Option Add(UniqueIndex uniqueIndex)
    {
        lock (_syncLock)
        {
            if (!_index.TryGetValue(uniqueIndex.IndexName, out var index))
            {
                index = new Dictionary<string, UniqueIndex>(StringComparer.OrdinalIgnoreCase);
                _index[uniqueIndex.IndexName] = index;
            }

            if (index.ContainsKey(uniqueIndex.Value)) return (StatusCode.Conflict, $"Index={uniqueIndex.IndexName} value={uniqueIndex.Value} already exist");
            index[uniqueIndex.Value] = uniqueIndex;

            _nodeKeyLookup.Add(uniqueIndex);
            return StatusCode.OK;
        }
    }

    private IEnumerable<(bool isRemove, string indexName, string tagValue)> GetIndexValues(IEnumerable<KeyValuePair<string, string?>> tagsWithValue, IEnumerable<string> indexes)
    {
        var list = indexes.Where(x => x.IsNotEmpty() && x != "-")
            .Select(x => (isRemove: x[0] == '-', indexName: x[0] == '-' ? x[1..] : x))
            .Join(
                tagsWithValue.Where(x => x.Value.IsNotEmpty()),
                x => x.indexName,
                x => x.Key,
                (i, tag) => (i.isRemove, i.indexName, tagValue: tag.Value.NotEmpty()),
                StringComparer.OrdinalIgnoreCase
                )
            .ToArray();

        return list;
    }
}