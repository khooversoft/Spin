//using System;
//using System.Buffers;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Data;

//public class IndexManager
//{
//    private readonly object _lock = new object();

//    public ConcurrentDictionary<string, IIndexHead> IndexHeads { get; } = new(StringComparer.OrdinalIgnoreCase);

//    public Option Add(string indexName, bool unique, string tagKey, string referenceKey)
//    {
//        indexName.NotEmpty();
//        tagKey.NotEmpty();
//        referenceKey.NotEmpty();

//        lock (_lock)
//        {
//            if (!IndexHeads.TryGetValue(indexName, out IIndexHead? indexHead))
//            {
//                indexHead = unique switch
//                {
//                    false => new IndexHead { Name = indexName },
//                    true => new UniqueIndexHead { Name = indexName },
//                };

//                IndexHeads[indexName] = indexHead;
//                return StatusCode.OK;
//            }

//            switch (indexHead)
//            {
//                case UniqueIndexHead uniqueIndex:
//                    var status = uniqueIndex.UniqueIndexToKey.TryAdd(tagKey, referenceKey);
//                    return status ? StatusCode.OK : (StatusCode.Conflict, $"Key={tagKey} already exist");

//                case IndexHead index:
//                    if( index.IndexToKeys.TryGetValue(tagKey, out HashSet<string>? values))
//                    {
//                        if (values.Contains(referenceKey)) return StatusCode.OK;

//                        values.Add(referenceKey);
//                        return StatusCode.OK;
//                    }

//                    index.IndexToKeys[tagKey] = new HashSet<string>(new string[] { referenceKey }, StringComparer.OrdinalIgnoreCase);
//                    break;
//            }

//            return StatusCode.OK;
//        }
//    }

//    public Option<string> Lookup(string indexName, string tagKey)
//    {
//        indexName.NotEmpty();
//        tagKey.NotEmpty();

//        lock (_lock)
//        {
//            if (!IndexHeads.TryGetValue(indexName, out IIndexHead? indexHead)) return StatusCode.NotFound;

//            switch (indexHead)
//            {
//                case UniqueIndexHead uniqueIndex:
//                    if (uniqueIndex.UniqueIndexToKey.TryGetValue(tagKey, out string? value))
//                    {
//                        return value;
//                    }

//                    return StatusCode.NotFound;

//                case IndexHead index:
//                    if (index.IndexToKeys.TryGetValue(tagKey, out HashSet<string>? values))
//                    {
//                        return values.ToImmutableArray();
//                    }

//                    return StatusCode.NotFound;
//            }
//        }
//    }
//}

//public interface IIndexHead { }

//public sealed class UniqueIndexHead : IIndexHead
//{
//    public string Name { get; init; } = null!;
//    public Dictionary<string, string> UniqueIndexToKey { get; init; } = new(StringComparer.OrdinalIgnoreCase);
//}

//public sealed class IndexHead : IIndexHead
//{
//    public string Name { get; init; } = null!;
//    public Dictionary<string, HashSet<string>> IndexToKeys { get; init; } = new(StringComparer.OrdinalIgnoreCase);
//}
