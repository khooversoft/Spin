using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;

namespace Toolbox.Test.Data;

public class DictionaryHashSetTests
{
    [Fact]
    public void Empty()
    {
        var set = new DictionaryHashSet<int, Guid>();
        set.Count.Should().Be(0);
        set.Count().Should().Be(0);

        set.Remove(5).Count().Should().Be(0);
    }

    [Fact]
    public void Single()
    {
        int key = 5;
        Guid refKey = Guid.NewGuid();

        var set = new DictionaryHashSet<int, Guid>().Set(key, refKey);

        set.Count.Should().Be(1);
        set.Count().Should().Be(1);
        set.First().Key.Should().Be(key);
        set.First().Value.Should().Be(refKey);

        set.Remove(key).Count().Should().Be(1);
    }

    [Fact]
    public void OneToMany()
    {
        var keys = Enumerable.Range(0, 5).ToArray();
        Guid refKey = Guid.NewGuid();

        var set = new DictionaryHashSet<int, Guid>();
        keys.ForEach(x => set.Set(x, refKey));

        set.Count.Should().Be(keys.Length);
        set.Count().Should().Be(keys.Length);

        var shuffle = keys.Shuffle().ToStack();

        do
        {
            var shouldMatch = shuffle
                .Select(x => new KeyValuePair<int, Guid>(x, refKey))
                .OrderBy(x => x.Key)
                .ToArray();

            var inSet = set.OrderBy(x => x.Key).ToArray();

            Enumerable.SequenceEqual(inSet, shouldMatch).Should().BeTrue();

            shuffle.TryPop(out var key).Should().BeTrue();

            var removeSet = set.Remove(key);
            removeSet.Count.Should().Be(1);
            removeSet.First().Should().Be(refKey);

        }
        while (shuffle.Count > 0);

    }

    [Fact]
    public void ManyToOne()
    {
        var key = 5;
        var refKeys = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToArray();

        var set = new DictionaryHashSet<int, Guid>();
        refKeys.ForEach(x => set.Set(key, x));

        set.Count.Should().Be(1);
        set.Count().Should().Be(refKeys.Length);

        var shuffle = refKeys.Shuffle().ToStack();

        do
        {
            var shouldMatch = shuffle
                .Select(x => new KeyValuePair<int, Guid>(key, x))
                .OrderBy(x => x.Value)
                .ToArray();

            var inSet = set.OrderBy(x => x.Value).ToArray();

            Enumerable.SequenceEqual(inSet, shouldMatch).Should().BeTrue();
            shuffle.TryPop(out var refKey).Should().BeTrue();

            set.Remove(key, refKey).Should().BeTrue();

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

        var set = new DictionaryHashSet<int, Guid>();
        baseSet.ForEach(x => set.Set(x.Key, x.Value));

        set.Count.Should().Be(keys.Length);
        set.Count().Should().Be(keys.Length * refKeys.Length);

        var shuffle = baseSet.Shuffle().ToStack();

        do
        {
            var shouldMatch = shuffle
                .OrderBy(x => x.Key).ThenBy(x => x.Value)
                .ToArray();

            var inSet = set.OrderBy(x => x.Key).ThenBy(x => x.Value).ToArray();
            Enumerable.SequenceEqual(inSet, shouldMatch).Should().BeTrue();

            shuffle.TryPop(out var pair).Should().BeTrue();
            set.Remove(pair.Key, pair.Value).Should().BeTrue();

        } while (shuffle.Count > 0);

    }
}
