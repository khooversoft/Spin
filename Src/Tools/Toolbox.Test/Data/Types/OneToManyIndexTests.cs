using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Test.Data.Types;

public class OneToManyIndexTests
{
    [Fact]
    public void Empty()
    {
        var set = new OneToManyIndex<int, Guid>();
        set.Count.Be(0);
        set.Count().Be(0);

        set.Remove(5).Count().Be(0);
    }

    [Fact]
    public void Single()
    {
        int key = 5;
        Guid refKey = Guid.NewGuid();

        var set = new OneToManyIndex<int, Guid>().Set(key, refKey);

        set.Count.Be(1);
        set.Count().Be(1);
        set.First().Key.Be(key);
        set.First().Value.Assert(x => x == refKey);

        set.Remove(key).Count().Be(1);
    }

    [Fact]
    public void OneToMany()
    {
        var keys = Enumerable.Range(0, 5).ToArray();
        Guid refKey = Guid.NewGuid();

        var set = new OneToManyIndex<int, Guid>();
        keys.ForEach(x => set.Set(x, refKey));

        set.Count.Be(keys.Length);
        set.Count().Be(keys.Length);

        var shuffle = keys.Shuffle().ToStack();

        do
        {
            var shouldMatch = shuffle
                .Select(x => new KeyValuePair<int, Guid>(x, refKey))
                .OrderBy(x => x.Key)
                .ToArray();

            var inSet = set.OrderBy(x => x.Key).ToArray();

            inSet.SequenceEqual(shouldMatch).BeTrue();

            shuffle.TryPop(out var key).BeTrue();

            var removeSet = set.Remove(key);
            removeSet.Count.Be(1);
            removeSet.First().Assert(x => x == refKey);

        }
        while (shuffle.Count > 0);

    }

    [Fact]
    public void ManyToOne()
    {
        var key = 5;
        var refKeys = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToArray();

        var set = new OneToManyIndex<int, Guid>();
        refKeys.ForEach(x => set.Set(key, x));

        set.Count.Be(1);
        set.Count().Be(refKeys.Length);

        var shuffle = refKeys.Shuffle().ToStack();

        do
        {
            var shouldMatch = shuffle
                .Select(x => new KeyValuePair<int, Guid>(key, x))
                .OrderBy(x => x.Value)
                .ToArray();

            var inSet = set.OrderBy(x => x.Value).ToArray();

            inSet.SequenceEqual(shouldMatch).BeTrue();
            shuffle.TryPop(out var refKey).BeTrue();

            set.Remove(key, refKey).BeTrue();

        } while (shuffle.Count > 0);
    }

    [Fact]
    public void ManyToMany()
    {
        var keys = Enumerable.Range(0, 5).ToArray();
        var refKeys = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToArray();

        var baseSet = keys
            .Select(x => refKeys.Select(y => new KeyValuePair<int, Guid>(x, y)))
            .SelectMany(x => x)
            .ToArray();

        var set = new OneToManyIndex<int, Guid>();
        baseSet.ForEach(x => set.Set(x.Key, x.Value));

        set.Count.Be(keys.Length);
        set.Count().Be(keys.Length * refKeys.Length);

        var shuffle = baseSet.Shuffle().ToStack();

        do
        {
            var shouldMatch = shuffle
                .OrderBy(x => x.Key).ThenBy(x => x.Value)
                .ToArray();

            var inSet = set.OrderBy(x => x.Key).ThenBy(x => x.Value).ToArray();
            inSet.SequenceEqual(shouldMatch).BeTrue();

            shuffle.TryPop(out var pair).BeTrue();
            set.Remove(pair.Key, pair.Value).BeTrue();

        } while (shuffle.Count > 0);

    }

    // NEW: Indexer, Get/TryGetValue, Clear, and duplicate Set idempotency + Remove false cases
    [Fact]
    public void Indexer_Get_TryGetValue_Clear_AndDuplicateSet()
    {
        var set = new OneToManyIndex<int, int>();

        set.Set(1, 100).Set(1, 101).Set(2, 200).Set(3, 300).Set(3, 300); // duplicate should be ignored

        // Indexer
        var k1 = set[1];
        k1.Count.Be(2);
        k1.OrderBy(x => x).SequenceEqual(new[] { 100, 101 }).BeTrue();

        var k2 = set[2];
        k2.Count.Be(1);
        k2[0].Be(200);

        var missing = set[999];
        missing.Count.Be(0);

        // Get and TryGetValue
        var g1 = set.Get(1);
        g1.OrderBy(x => x).SequenceEqual(new[] { 100, 101 }).BeTrue();

        set.TryGetValue(1, out var tv1).BeTrue();
        tv1!.OrderBy(x => x).SequenceEqual(new[] { 100, 101 }).BeTrue();

        set.TryGetValue(500, out var tvMissing).BeFalse();
        (tvMissing == null).BeTrue();

        // Duplicate idempotency
        set[3].Count.Be(1);

        // Remove by key returns removed list
        var removed2 = set.Remove(2);
        removed2.Count.Be(1);
        removed2[0].Be(200);
        set[2].Count.Be(0);

        // Remove pair false-case
        set.Remove(3, 999).BeFalse();

        // Clear
        set.Clear();
        set.Count.Be(0);
        set.Count().Be(0);
        set[1].Count.Be(0);
        set.Get(1).Count.Be(0);
        set.TryGetValue(1, out var tvAfterClear).BeFalse();
        (tvAfterClear == null).BeTrue();
    }

    // NEW: Custom comparers for key and reference
    [Fact]
    public void CustomComparers_CaseInsensitive()
    {
        var set = new OneToManyIndex<string, string>(StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);

        set.Set("User", "ID-001");
        set.Set("user", "id-001"); // should collapse to same key/ref due to case-insensitive comparers

        set.Count.Be(1);
        set.Count().Be(1);

        var vals = set["USER"];
        vals.Count.Be(1);
        vals[0].Be("ID-001"); // original casing of first insert preserved
    }

    // NEW: Concurrency stress: mixed adds/removes/lookups under parallel load
    [Fact]
    public async Task Concurrent_ReadWrite_Stress()
    {
        var set = new OneToManyIndex<int, int>();

        const int keyDomain = 64;
        const int refDomain = 64;

        int workerCount = Math.Max(4, Environment.ProcessorCount / 2);
        int opsPerWorker = 10_000;

        var tasks = Enumerable.Range(0, workerCount).Select(worker => Task.Run(() =>
        {
            var r = new Random(12345 + worker);

            for (int i = 0; i < opsPerWorker; i++)
            {
                int k = r.Next(keyDomain);
                int v = r.Next(refDomain);

                switch (i % 5)
                {
                    case 0:
                    case 1:
                        set.Set(k, v);
                        break;
                    case 2:
                        set.Remove(k, v);
                        break;
                    case 3:
                        set.Remove(k);
                        break;
                    case 4:
                        // reads
                        _ = set[k].Count;
                        _ = set.Count;
                        _ = set.Count();
                        break;
                }
            }
        }));

        await Task.WhenAll(tasks);

        // Validate invariants after operations complete
        ValidateInvariants(set);
    }

    // NEW: Enumeration must be safe while writes occur (snapshot semantics)
    [Fact]
    public async Task Concurrent_Enumeration_SnapshotSafe()
    {
        var set = new OneToManyIndex<int, int>();

        // Seed
        for (int k = 0; k < 16; k++)
        {
            for (int v = 0; v < 4; v++) set.Set(k, k * 10 + v);
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        var writer = Task.Run(async () =>
        {
            var r = new Random(8675309);
            while (!cts.IsCancellationRequested)
            {
                int k = r.Next(0, 64);
                int v = r.Next(0, 64);
                switch (r.Next(3))
                {
                    case 0: set.Set(k, v); break;
                    case 1: set.Remove(k, v); break;
                    case 2: set.Remove(k); break;
                }
                await Task.Yield();
            }
        }, cts.Token);

        int enumerations = 0;
        var reader = Task.Run(async () =>
        {
            while (!cts.IsCancellationRequested)
            {
                // Should not throw or hang and should not contain duplicates
                var snapshot = set.ToArray();
                snapshot.Length.Be(snapshot.Distinct().Count());
                enumerations++;
                await Task.Yield();
            }
        }, cts.Token);

        await Task.Delay(1000);
        cts.Cancel();
        await Task.WhenAll(Task.WhenAll(writer.ContinueWith(_ => { })), Task.WhenAll(reader.ContinueWith(_ => { })));

        enumerations.Assert(x => x > 0, x => $"{x} must be > 0");

        // Final check
        ValidateInvariants(set);
    }

    private static void ValidateInvariants<TKey, TR>(OneToManyIndex<TKey, TR> set)
        where TKey : notnull
        where TR : notnull
    {
        var pairs = set.ToArray();

        // Total pair count equals enumeration count
        pairs.Length.Be(set.Count());

        // Distinct key count equals Count property
        pairs.Select(x => x.Key).Distinct().Count().Be(set.Count);

        // No duplicate pairs
        pairs.Length.Be(pairs.Distinct().Count());

        // Per-key values match indexer snapshot
        foreach (var group in pairs.GroupBy(x => x.Key))
        {
            var expected = group.Select(x => x.Value).OrderBy(x => x).ToArray();
            var actual = set[group.Key].OrderBy(x => x).ToArray();
            expected.SequenceEqual(actual).BeTrue();
        }
    }
}
