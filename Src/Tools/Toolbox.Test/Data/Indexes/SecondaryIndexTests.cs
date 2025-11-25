using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Test.Data.Indexes;

public class SecondaryIndexTests
{
    [Fact]
    public void SecondaryIndexEmptyTest()
    {
        var index = new SecondaryIndex<int, Guid>();

        index.Count.Be(0);
        index.Count().Be(0);

        var keys = index.LookupPrimaryKey(Guid.NewGuid());
        keys.Count.Be(0);

        var pkeys = index.Lookup(99);
        pkeys.Count.Be(0);

        index.Remove(2, Guid.NewGuid()).BeFalse();
    }

    [Fact]
    public void SecondaryIndexSimpleTest()
    {
        var pkey1 = Guid.NewGuid();
        var key = 5;

        var index = new SecondaryIndex<int, Guid>()
            .Set(key, pkey1);

        index.Count.Be(1);
        index.Count().Be(1);
        index.First().Key.Be(key);
        index.First().Value.Assert(x => x == pkey1);

        var keys = index.LookupPrimaryKey(pkey1);
        keys.Count.Be(1);
        keys[0].Be(key);

        var badPrimaryKeyLookup = index.LookupPrimaryKey(Guid.NewGuid());
        badPrimaryKeyLookup.Count.Be(0);

        var pkeys = index.Lookup(key);
        pkeys.Count.Be(1);
        pkeys[0].Assert(x => x == pkey1);

        var badLookup = index.Lookup(99);
        badLookup.Count.Be(0);

        index.Remove(key, pkey1).BeTrue();
        index.Count.Be(0);
        index.Count().Be(0);

        index.Lookup(key).Count.Be(0);
        index.LookupPrimaryKey(pkey1).Count.Be(0);
    }

    [Fact]
    public void SecondaryIndexOneToManyTest()
    {
        var pkeyInput = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToArray();
        var key = 5;

        var index = new SecondaryIndex<int, Guid>();
        pkeyInput.ForEach(x => index.Set(key, x));

        index.Count.Be(1);

        var shuffle = pkeyInput.Shuffle().ToStack();

        do
        {
            shuffle.Count.Assert(x => x > 0, x => $"{x} must be > 0");
            pkeyInput.Length.Assert(x => x > 0, x => $"{x} must be > 0");
            index.Count().Be(pkeyInput.Length);

            var shouldMatch = pkeyInput
                .Select(x => new KeyValuePair<int, Guid>(key, x))
                .OrderBy(x => x.Key)
                .ThenBy(x => x.Value)
                .ToArray();

            shouldMatch.Length.Be(pkeyInput.Length);

            index
                .OrderBy(x => x.Key)
                .ThenBy(x => x.Value)
                .ToArray()
                .SequenceEqual(shouldMatch)
                .BeTrue();

            var keys = index.LookupPrimaryKey(shuffle.Peek());
            keys.Count.Be(1);
            keys[0].Be(key);

            var pkeys = index.Lookup(key);
            pkeys.Count.Be(pkeyInput.Length);
            pkeyInput.OrderBy(x => x).SequenceEqual(pkeys.OrderBy(x => x)).BeTrue();

            shuffle.TryPop(out var pkey).BeTrue();
            index.Remove(key, pkey).BeTrue();

            index.LookupPrimaryKey(pkey).Count.Be(0);
            index.Where(x => x.Key == key && x.Value == pkey).Count().Be(0);

            pkeyInput = pkeyInput.Where(x => x != pkey).ToArray();
        }
        while (pkeyInput.Length > 0);
    }

    [Fact]
    public void SecondaryIndexManyToOneTest()
    {
        var pkeyInput = Guid.NewGuid();
        var keyInput = Enumerable.Range(0, 5).ToArray();

        var index = new SecondaryIndex<int, Guid>();
        keyInput.ForEach(x => index.Set(x, pkeyInput));

        var shuffle = keyInput.Shuffle().ToStack();

        do
        {
            shuffle.Count.Assert(x => x > 0, x => $"{x} must be > 0");
            keyInput.Length.Assert(x => x > 0, x => $"{x} must be > 0");

            index.Count.Be(shuffle.Count);
            index.Count().Be(shuffle.Count);

            var shouldMatch = shuffle.Select(x => new KeyValuePair<int, Guid>(x, pkeyInput)).OrderBy(x => x.Key).ToArray();
            shouldMatch.Length.Be(shuffle.Count);
            index.OrderBy(x => x.Key).ToArray().SequenceEqual(shouldMatch).BeTrue();

            var keys = index.LookupPrimaryKey(pkeyInput);
            keys.Count.Be(shuffle.Count);
            shuffle.OrderBy(x => x).SequenceEqual(keys.OrderBy(x => x)).BeTrue();

            var pkeys = index.Lookup(shuffle.Peek());
            pkeys.Count.Be(1);
            pkeys[0].Assert(x => x == pkeyInput);

            shuffle.TryPop(out var key).BeTrue();
            index.Remove(key, pkeyInput).BeTrue();

            index.Lookup(key).Count.Be(0);
            index.Where(x => x.Key == key && x.Value == pkeyInput).Count().Be(0);
        }
        while (shuffle.Count > 0);
    }

    [Fact]
    public void SecondaryIndexManyToManyTest()
    {
        var keyInput = Enumerable.Range(0, 5).ToArray();
        var pkeyInput = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToArray();

        var index = new SecondaryIndex<int, Guid>();

        var masterList = keyInput
            .Select(x => pkeyInput.Select(y => new KeyValuePair<int, Guid>(x, y)))
            .SelectMany(x => x)
            .ToArray();

        masterList.ForEach(x => index.Set(x.Key, x.Value));

        var shuffle = masterList.Shuffle().ToStack();

        do
        {
            shuffle.Count.Assert(x => x > 0, x => $"{x} must be > 0");
            keyInput.Length.Assert(x => x > 0, x => $"{x} must be > 0");

            index.Count.Be(shuffle.Select(x => x.Key).Distinct().Count());
            index.Count().Be(shuffle.Count);

            index.OrderBy(x => x.Key).ThenBy(x => x.Value).SequenceEqual(
                shuffle.OrderBy(x => x.Key).ThenBy(x => x.Value)
                ).BeTrue();

            Guid primaryKey = shuffle.Peek().Value;
            var keys = index.LookupPrimaryKey(primaryKey);

            var shouldMatchPk = shuffle
                .Where(x => x.Value == primaryKey)
                .Select(x => x.Key)
                .OrderBy(x => x)
                .ToArray();

            keys.Count.Be(shouldMatchPk.Length);
            shouldMatchPk.SequenceEqual(keys.OrderBy(x => x)).BeTrue();

            int key = shuffle.Peek().Key;
            var pkeys = index.Lookup(key);

            var shouldMatch = shuffle
                .Where(x => x.Key == key)
                .Select(x => x.Value)
                .OrderBy(x => x)
                .ToArray();

            pkeys.Count.Be(shouldMatch.Length);
            var inSet = pkeys.OrderBy(x => x).ToArray();

            inSet.SequenceEqual(shouldMatch).BeTrue();

            shuffle.TryPop(out var item).BeTrue();
            index.Remove(item.Key, item.Value).BeTrue();

            index.Lookup(item.Key).Count.Be(shuffle.Where(x => x.Key == item.Key).Count());
            index.LookupPrimaryKey(item.Value).Count.Be(shuffle.Where(x => x.Value == item.Value).Count());
        }
        while (shuffle.Count > 0);
    }

    [Fact]
    public void SecondaryIndexManyToManySameKeyTypeTest()
    {
        var keyInput = Enumerable.Range(0, 5).Select(x => x.ToString()).ToArray();
        var pkeyInput = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid().ToString()).ToArray();

        var index = new SecondaryIndex<string, string>();

        var masterList = keyInput
            .Select(x => pkeyInput.Select(y => new KeyValuePair<string, string>(x, y)))
            .SelectMany(x => x)
            .ToArray();

        masterList.ForEach(x => index.Set(x.Key, x.Value));

        var shuffle = masterList.Shuffle().ToStack();

        do
        {
            shuffle.Count.Assert(x => x > 0, x => $"{x} must be > 0");
            keyInput.Length.Assert(x => x > 0, x => $"{x} must be > 0");

            index.Count.Be(shuffle.Select(x => x.Key).Distinct().Count());
            index.Count().Be(shuffle.Count);

            index.OrderBy(x => x.Key).ThenBy(x => x.Value).SequenceEqual(
                shuffle.OrderBy(x => x.Key).ThenBy(x => x.Value)
                ).BeTrue();

            string primaryKey = shuffle.Peek().Value;
            var keys = index.LookupPrimaryKey(primaryKey);

            var shouldMatchPk = shuffle
                .Where(x => x.Value == primaryKey)
                .Select(x => x.Key)
                .OrderBy(x => x)
                .ToArray();

            keys.Count.Be(shouldMatchPk.Length);
            shouldMatchPk.SequenceEqual(keys.OrderBy(x => x)).BeTrue();

            string key = shuffle.Peek().Key;
            var pkeys = index.Lookup(key);

            var shouldMatch = shuffle
                .Where(x => x.Key == key)
                .Select(x => x.Value)
                .OrderBy(x => x)
                .ToArray();

            pkeys.Count.Be(shouldMatch.Length);
            shouldMatch.SequenceEqual(pkeys.OrderBy(x => x)).BeTrue();

            shuffle.TryPop(out var item).BeTrue();
            index.Remove(item.Key, item.Value).BeTrue();

            index.Lookup(item.Key).Count.Be(shuffle.Where(x => x.Key == item.Key).Count());
            index.LookupPrimaryKey(item.Value).Count.Be(shuffle.Where(x => x.Value == item.Value).Count());
        }
        while (shuffle.Count > 0);
    }

    // NEW: Clear, Remove(key), RemovePrimaryKey, indexer coverage
    [Fact]
    public void SecondaryIndex_ClearAndRemoveVariants_AndIndexerTest()
    {
        var index = new SecondaryIndex<int, int>();

        // Add 3 keys with varying cardinality
        index.Set(1, 100).Set(1, 101).Set(2, 200).Set(3, 300).Set(3, 301).Set(3, 302);

        // Indexer returns read-only list; verify contents
        var k1 = index[1];
        k1.Count.Be(2);
        k1.OrderBy(x => x).SequenceEqual(new[] { 100, 101 }).BeTrue();

        var k2 = index[2];
        k2.Count.Be(1);
        k2[0].Be(200);

        var kMissing = index[999];
        kMissing.Count.Be(0);

        // Remove by key
        index.Remove(2).BeTrue();
        index.Lookup(2).Count.Be(0);
        index.LookupPrimaryKey(200).Count.Be(0);
        index.Count.Be(2); // keys: 1,3

        // Remove by primary key
        index.RemovePrimaryKey(301).BeTrue();
        index.LookupPrimaryKey(301).Count.Be(0);
        index.Lookup(3).OrderBy(x => x).SequenceEqual(new[] { 300, 302 }).BeTrue();

        // Clear everything
        index.Clear();
        index.Count.Be(0);
        index.Count().Be(0);
        index.Lookup(1).Count.Be(0);
        index.LookupPrimaryKey(300).Count.Be(0);
    }

    // NEW: Duplicate Set should be idempotent (no duplicate pairs)
    [Fact]
    public void SecondaryIndex_DuplicateSet_IsIdempotent()
    {
        var index = new SecondaryIndex<string, Guid>();
        var key = "K";
        var p = Guid.NewGuid();

        index.Set(key, p).Set(key, p).Set(key, p);

        index.Count.Be(1);
        index.Count().Be(1);
        index.Lookup(key).Count.Be(1);
        index.Lookup(key)[0].Assert(x => x == p);

        var ks = index.LookupPrimaryKey(p);
        ks.Count.Be(1);
        ks[0].Be(key);
    }

    // NEW: Custom comparers on both key and primary key
    [Fact]
    public void SecondaryIndex_CustomComparers_CaseInsensitive()
    {
        var index = new SecondaryIndex<string, string>(StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);

        index.Set("User", "ID-001");
        index.Set("user", "id-001"); // same using case-insensitive comparers

        index.Count.Be(1);
        index.Count().Be(1);

        index.Lookup("USER").Single().Be("ID-001");
        index.LookupPrimaryKey("Id-001").Single().Be("User"); // original casing may vary; comparer must unify mapping
    }

    // NEW: Concurrency stress: mixed adds/removes/lookups under parallel load
    [Fact]
    public async Task SecondaryIndex_Concurrent_ReadWrite_Stress()
    {
        var index = new SecondaryIndex<int, int>();

        // small domains to increase contention/cross-links
        const int keyDomain = 64;
        const int pkeyDomain = 64;

        int WorkerCount = Math.Max(4, Environment.ProcessorCount / 2);
        int OpsPerWorker = 10_000;

        var rnd = new Random(12345);
        //int NextKey() => rnd.Next(keyDomain);
        //int NextP() => rnd.Next(pkeyDomain);

        var tasks = Enumerable.Range(0, WorkerCount).Select(worker => Task.Run(() =>
        {
            var lr = new Random(12345 + worker); // deterministic per worker

            for (int i = 0; i < OpsPerWorker; i++)
            {
                int k = lr.Next(keyDomain);
                int p = lr.Next(pkeyDomain);

                switch (i % 6)
                {
                    case 0:
                    case 1:
                        index.Set(k, p);
                        break;
                    case 2:
                        index.Remove(k, p);
                        break;
                    case 3:
                        index.Remove(k);
                        break;
                    case 4:
                        index.RemovePrimaryKey(p);
                        break;
                    case 5:
                        // Reads
                        _ = index.Lookup(k).Count;
                        _ = index.LookupPrimaryKey(p).Count;
                        break;
                }
            }
        }));

        await Task.WhenAll(tasks);

        // Validate invariants after all operations complete (no concurrency at this point)
        ValidateInvariants(index);
    }

    private static void ValidateInvariants<TKey, TP>(SecondaryIndex<TKey, TP> index)
        where TKey : notnull
        where TP : notnull
    {
        // Snapshot pairs
        var pairs = index.ToArray();

        // Count of pairs
        pairs.Length.Be(index.Count());

        // Distinct key count equals index.Count
        pairs.Select(x => x.Key).Distinct().Count().Be(index.Count);

        // For each key, Lookups should match snapshot groups
        foreach (var group in pairs.GroupBy(x => x.Key))
        {
            var expected = group.Select(x => x.Value).OrderBy(x => x).ToArray();
            var actual = index.Lookup(group.Key).OrderBy(x => x).ToArray();
            expected.SequenceEqual(actual).BeTrue();
        }

        // For each primary key, reverse lookup should match snapshot groups
        foreach (var group in pairs.GroupBy(x => x.Value))
        {
            var expected = group.Select(x => x.Key).OrderBy(x => x).ToArray();
            var actual = index.LookupPrimaryKey(group.Key).OrderBy(x => x).ToArray();
            expected.SequenceEqual(actual).BeTrue();
        }
    }
}
