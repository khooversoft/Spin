using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Should;

namespace Toolbox.Test.Data.Types;

public class SecondaryIndexTests
{
    [Fact]
    public void SecondaryIndexEmptyTest()
    {
        var index = new SecondaryIndex<int, Guid>();

        index.Count.Should().Be(0);
        index.Count().Should().Be(0);

        var keys = index.LookupPrimaryKey(Guid.NewGuid());
        keys.Count.Should().Be(0);

        var pkeys = index.Lookup(99);
        pkeys.Count.Should().Be(0);

        index.Remove(2, Guid.NewGuid()).Should().BeFalse();
    }

    [Fact]
    public void SecondaryIndexSimpleTest()
    {
        var pkey1 = Guid.NewGuid();
        var key = 5;

        var index = new SecondaryIndex<int, Guid>()
            .Set(key, pkey1);

        index.Count.Should().Be(1);
        index.Count().Should().Be(1);
        index.First().Key.Should().Be(key);
        index.First().Value.Assert(x => x == pkey1);

        var keys = index.LookupPrimaryKey(pkey1);
        keys.Count.Should().Be(1);
        keys[0].Should().Be(key);

        var badPrimaryKeyLookup = index.LookupPrimaryKey(Guid.NewGuid());
        badPrimaryKeyLookup.Count.Should().Be(0);

        var pkeys = index.Lookup(key);
        pkeys.Count.Should().Be(1);
        pkeys[0].Assert(x => x == pkey1);

        var badLookup = index.Lookup(99);
        badLookup.Count.Should().Be(0);

        index.Remove(key, pkey1).Should().BeTrue();
        index.Count.Should().Be(0);
        index.Count().Should().Be(0);

        index.Lookup(key).Count.Should().Be(0);
        index.LookupPrimaryKey(pkey1).Count.Should().Be(0);
    }

    [Fact]
    public void SecondaryIndexOneToManyTest()
    {
        var pkeyInput = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToArray();
        var key = 5;

        var index = new SecondaryIndex<int, Guid>();
        pkeyInput.ForEach(x => index.Set(key, x));

        index.Count.Should().Be(1);

        var shuffle = pkeyInput.Shuffle().ToStack();

        do
        {
            shuffle.Count.Assert(x => x > 0, x => $"{x} must be > 0");
            pkeyInput.Length.Assert(x => x > 0, x => $"{x} must be > 0");
            index.Count().Should().Be(pkeyInput.Length);

            var shouldMatch = pkeyInput.Select(x => new KeyValuePair<int, Guid>(key, x)).OrderBy(x => x.Key).ToArray();
            shouldMatch.Length.Should().Be(pkeyInput.Length);
            index.OrderBy(x => x.Key).ToArray().SequenceEqual(shouldMatch).Should().BeTrue();

            var keys = index.LookupPrimaryKey(shuffle.Peek());
            keys.Count.Should().Be(1);
            keys[0].Should().Be(key);

            var pkeys = index.Lookup(key);
            pkeys.Count.Should().Be(pkeyInput.Length);
            pkeyInput.OrderBy(x => x).SequenceEqual(pkeys.OrderBy(x => x)).Should().BeTrue();

            shuffle.TryPop(out var pkey).Should().BeTrue();
            index.Remove(key, pkey).Should().BeTrue();

            index.LookupPrimaryKey(pkey).Count.Should().Be(0);
            index.Where(x => x.Key == key && x.Value == pkey).Count().Should().Be(0);

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

            index.Count.Should().Be(shuffle.Count);
            index.Count().Should().Be(shuffle.Count);

            var shouldMatch = shuffle.Select(x => new KeyValuePair<int, Guid>(x, pkeyInput)).OrderBy(x => x.Key).ToArray();
            shouldMatch.Length.Should().Be(shuffle.Count);
            index.OrderBy(x => x.Key).ToArray().SequenceEqual(shouldMatch).Should().BeTrue();

            var keys = index.LookupPrimaryKey(pkeyInput);
            keys.Count.Should().Be(shuffle.Count);
            shuffle.OrderBy(x => x).SequenceEqual(keys.OrderBy(x => x)).Should().BeTrue();

            var pkeys = index.Lookup(shuffle.Peek());
            pkeys.Count.Should().Be(1);
            pkeys[0].Assert(x => x == pkeyInput);

            shuffle.TryPop(out var key).Should().BeTrue();
            index.Remove(key, pkeyInput).Should().BeTrue();

            index.Lookup(key).Count.Should().Be(0);
            index.Where(x => x.Key == key && x.Value == pkeyInput).Count().Should().Be(0);
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

            index.Count.Should().Be(shuffle.Select(x => x.Key).Distinct().Count());
            index.Count().Should().Be(shuffle.Count);

            index.OrderBy(x => x.Key).ThenBy(x => x.Value).SequenceEqual(
                shuffle.OrderBy(x => x.Key).ThenBy(x => x.Value)
                ).Should().BeTrue();

            Guid primaryKey = shuffle.Peek().Value;
            var keys = index.LookupPrimaryKey(primaryKey);

            var shouldMatchPk = shuffle
                .Where(x => x.Value == primaryKey)
                .Select(x => x.Key)
                .OrderBy(x => x)
                .ToArray();

            keys.Count.Should().Be(shouldMatchPk.Length);
            shouldMatchPk.SequenceEqual(keys.OrderBy(x => x)).Should().BeTrue();

            int key = shuffle.Peek().Key;
            var pkeys = index.Lookup(key);

            var shouldMatch = shuffle
                .Where(x => x.Key == key)
                .Select(x => x.Value)
                .OrderBy(x => x)
                .ToArray();

            pkeys.Count.Should().Be(shouldMatch.Length);
            var inSet = pkeys.OrderBy(x => x).ToArray();

            inSet.SequenceEqual(shouldMatch).Should().BeTrue();

            shuffle.TryPop(out var item).Should().BeTrue();
            index.Remove(item.Key, item.Value).Should().BeTrue();

            index.Lookup(item.Key).Count.Should().Be(shuffle.Where(x => x.Key == item.Key).Count());
            index.LookupPrimaryKey(item.Value).Count.Should().Be(shuffle.Where(x => x.Value == item.Value).Count());
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

            index.Count.Should().Be(shuffle.Select(x => x.Key).Distinct().Count());
            index.Count().Should().Be(shuffle.Count);

            index.OrderBy(x => x.Key).ThenBy(x => x.Value).SequenceEqual(
                shuffle.OrderBy(x => x.Key).ThenBy(x => x.Value)
                ).Should().BeTrue();

            string primaryKey = shuffle.Peek().Value;
            var keys = index.LookupPrimaryKey(primaryKey);

            var shouldMatchPk = shuffle
                .Where(x => x.Value == primaryKey)
                .Select(x => x.Key)
                .OrderBy(x => x)
                .ToArray();

            keys.Count.Should().Be(shouldMatchPk.Length);
            shouldMatchPk.SequenceEqual(keys.OrderBy(x => x)).Should().BeTrue();

            string key = shuffle.Peek().Key;
            var pkeys = index.Lookup(key);

            var shouldMatch = shuffle
                .Where(x => x.Key == key)
                .Select(x => x.Value)
                .OrderBy(x => x)
                .ToArray();

            pkeys.Count.Should().Be(shouldMatch.Length);
            shouldMatch.SequenceEqual(pkeys.OrderBy(x => x)).Should().BeTrue();

            shuffle.TryPop(out var item).Should().BeTrue();
            index.Remove(item.Key, item.Value).Should().BeTrue();

            index.Lookup(item.Key).Count.Should().Be(shuffle.Where(x => x.Key == item.Key).Count());
            index.LookupPrimaryKey(item.Value).Count.Should().Be(shuffle.Where(x => x.Value == item.Value).Count());
        }
        while (shuffle.Count > 0);
    }
}
