using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;

namespace Toolbox.Test.Data;

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
        index.First().Value.Should().Be(pkey1);

        var keys = index.LookupPrimaryKey(pkey1);
        keys.Count.Should().Be(1);
        keys[0].Should().Be(key);

        var badPrimaryKeyLookup = index.LookupPrimaryKey(Guid.NewGuid());
        badPrimaryKeyLookup.Count.Should().Be(0);

        var pkeys = index.Lookup(key);
        pkeys.Count.Should().Be(1);
        pkeys[0].Should().Be(pkey1);

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

        var shuffel = pkeyInput.Shuffle().ToStack();

        do
        {
            shuffel.Count.Should().BeGreaterThan(0);
            pkeyInput.Length.Should().BeGreaterThan(0);
            index.Count().Should().Be(pkeyInput.Length);

            var shouldMatch = pkeyInput.Select(x => new KeyValuePair<int, Guid>(key, x)).OrderBy(x => x.Key).ToArray();
            shouldMatch.Length.Should().Be(pkeyInput.Length);
            Enumerable.SequenceEqual(index.OrderBy(x => x.Key).ToArray(), shouldMatch).Should().BeTrue();

            var keys = index.LookupPrimaryKey(shuffel.Peek());
            keys.Count.Should().Be(1);
            keys[0].Should().Be(key);

            var pkeys = index.Lookup(key);
            pkeys.Count.Should().Be(pkeyInput.Length);
            Enumerable.SequenceEqual(pkeyInput.OrderBy(x => x), pkeys.OrderBy(x => x)).Should().BeTrue();

            shuffel.TryPop(out var pkey).Should().BeTrue();
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
            shuffle.Count.Should().BeGreaterThan(0);
            keyInput.Length.Should().BeGreaterThan(0);

            index.Count.Should().Be(shuffle.Count);
            index.Count().Should().Be(shuffle.Count);

            var shouldMatch = shuffle.Select(x => new KeyValuePair<int, Guid>(x, pkeyInput)).OrderBy(x => x.Key).ToArray();
            shouldMatch.Length.Should().Be(shuffle.Count);
            Enumerable.SequenceEqual(index.OrderBy(x => x.Key).ToArray(), shouldMatch).Should().BeTrue();

            var keys = index.LookupPrimaryKey(pkeyInput);
            keys.Count.Should().Be(shuffle.Count);
            Enumerable.SequenceEqual(shuffle.OrderBy(x => x), keys.OrderBy(x => x)).Should().BeTrue();

            var pkeys = index.Lookup(shuffle.Peek());
            pkeys.Count.Should().Be(1);
            pkeys[0].Should().Be(pkeyInput);

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
            shuffle.Count.Should().BeGreaterThan(0);
            keyInput.Length.Should().BeGreaterThan(0);

            index.Count.Should().Be(shuffle.Select(x => x.Key).Distinct().Count());
            index.Count().Should().Be(shuffle.Count);

            Enumerable.SequenceEqual(
                index.OrderBy(x => x.Key).ThenBy(x => x.Value),
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
            Enumerable.SequenceEqual(shouldMatchPk, keys.OrderBy(x => x)).Should().BeTrue();

            int key = shuffle.Peek().Key;
            var pkeys = index.Lookup(key);

            var shouldMatch = shuffle
                .Where(x => x.Key == key)
                .Select(x => x.Value)
                .OrderBy(x => x)
                .ToArray();

            pkeys.Count.Should().Be(shouldMatch.Length);
            var inSet = pkeys.OrderBy(x => x).ToArray();

            Enumerable.SequenceEqual(inSet, shouldMatch).Should().BeTrue();

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
            shuffle.Count.Should().BeGreaterThan(0);
            keyInput.Length.Should().BeGreaterThan(0);

            index.Count.Should().Be(shuffle.Select(x => x.Key).Distinct().Count());
            index.Count().Should().Be(shuffle.Count);

            Enumerable.SequenceEqual(
                index.OrderBy(x => x.Key).ThenBy(x => x.Value),
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
            Enumerable.SequenceEqual(shouldMatchPk, keys.OrderBy(x => x)).Should().BeTrue();

            string key = shuffle.Peek().Key;
            var pkeys = index.Lookup(key);

            var shouldMatch = shuffle
                .Where(x => x.Key == key)
                .Select(x => x.Value)
                .OrderBy(x => x)
                .ToArray();

            pkeys.Count.Should().Be(shouldMatch.Length);
            Enumerable.SequenceEqual(shouldMatch, pkeys.OrderBy(x => x)).Should().BeTrue();

            shuffle.TryPop(out var item).Should().BeTrue();
            index.Remove(item.Key, item.Value).Should().BeTrue();

            index.Lookup(item.Key).Count.Should().Be(shuffle.Where(x => x.Key == item.Key).Count());
            index.LookupPrimaryKey(item.Value).Count.Should().Be(shuffle.Where(x => x.Value == item.Value).Count());
        }
        while (shuffle.Count > 0);
    }
}
