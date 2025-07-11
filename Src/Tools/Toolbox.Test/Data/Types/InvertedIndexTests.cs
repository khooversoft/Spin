using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Test.Data.Types;

public class InvertedIndexTests
{
    [Fact]
    public void Empty()
    {
        var set = new InvertedIndex<int, Guid>();
        set.Count.Be(0);
        set.Count().Be(0);

        set.Remove(5).Count().Be(0);
    }

    [Fact]
    public void Single()
    {
        int key = 5;
        Guid refKey = Guid.NewGuid();

        var set = new InvertedIndex<int, Guid>().Set(key, refKey);

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

        var set = new InvertedIndex<int, Guid>();
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

        var set = new InvertedIndex<int, Guid>();
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

        var set = new InvertedIndex<int, Guid>();
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
}
