//using System.Collections.Frozen;
//using Toolbox.Tools;

//namespace Toolbox.Data;

//public class FrozenInvertedIndex<TKey, TReferenceKey>
//    where TKey : notnull
//    where TReferenceKey : notnull
//{
//    public FrozenInvertedIndex(FrozenDictionary<TKey, FrozenSet<TReferenceKey>> index) => Index = index.NotNull();

//    public FrozenDictionary<TKey, FrozenSet<TReferenceKey>> Index { get; }

//    public FrozenSet<TReferenceKey> Search(TKey key)
//    {
//        return Index.TryGetValue(key, out var documentIds) switch
//        {
//            false => FrozenSet<TReferenceKey>.Empty,
//            true => documentIds,
//        };
//    }
//}


//public static class FrozenInvertedIndexExtensions
//{
//    public static FrozenInvertedIndex<TKey, TReferenceKey> ToFrozenInvertedIndex<TKey, TReferenceKey>(this InvertedIndex<TKey, TReferenceKey> subject)
//        where TKey : notnull
//        where TReferenceKey : notnull
//    {
//        FrozenDictionary<TKey, FrozenSet<TReferenceKey>> frozen = subject
//            .GroupBy(x => x.Key)
//            .Select(x => new KeyValuePair<TKey, FrozenSet<TReferenceKey>>(x.Key, x.ToList().Select(x => x.Value).ToFrozenSet(subject.ReferenceComparer)))
//            .ToFrozenDictionary(subject.KeyComparer);

//        return new FrozenInvertedIndex<TKey, TReferenceKey>(frozen);
//    }
//}
