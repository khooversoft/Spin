using Toolbox.Tools;

namespace Toolbox.Graph.test.Policy;

public class GroupCollectionTests
{
    [Fact]
    public void Empty_Defaults()
    {
        var subject = new GroupCollection();

        subject.Count.Be(0);
        subject.IsReadOnly.BeFalse();

        subject.Contains("nope").BeFalse();
        subject.TryGetGroup("nope", out var _).BeFalse();

        subject.ToList().Count.Be(0);
    }

    [Fact]
    public void Add_Contains_Indexer_TryGet()
    {
        var subject = new GroupCollection();

        var g1 = new GroupPolicy("g1", new[] { "u1", "u2" });
        subject.Add(g1);

        subject.Count.Be(1);
        subject.Contains(g1).BeTrue();
        subject.Contains("g1").BeTrue();

        subject.TryGetGroup("g1", out var found).BeTrue();
        found.NameIdentifier.Be("g1");
        found.Members.Count.Be(2);
        found.Members.Contains("u2").BeTrue();

        // Indexer get/set and validation
        var g1Updated = new GroupPolicy("g1", new[] { "u1", "u3" });
        subject["g1"] = g1Updated;

        subject.TryGetGroup("g1", out var updated).BeTrue();
        updated.Members.Contains("u3").BeTrue();
        updated.Members.Contains("u2").BeFalse();
    }

    [Fact]
    public void Remove_And_Clear()
    {
        var subject = new GroupCollection();
        var g1 = new GroupPolicy("g1", new[] { "u1" });
        var g2 = new GroupPolicy("g2", new[] { "u2" });

        subject.Add(g1);
        subject.Add(g2);
        subject.Count.Be(2);

        subject.Remove(g1).BeTrue();
        subject.Count.Be(1);
        subject.Contains("g1").BeFalse();

        subject.Remove(g1).BeFalse(); // already removed

        subject.Clear();
        subject.Count.Be(0);
        subject.Contains("g2").BeFalse();
    }

    [Fact]
    public void CopyTo_Basic_And_Offset()
    {
        var subject = new GroupCollection();
        var g1 = new GroupPolicy("g1", new[] { "a" });
        var g2 = new GroupPolicy("g2", new[] { "b" });
        var g3 = new GroupPolicy("g3", new[] { "c" });

        subject.Add(g1);
        subject.Add(g2);
        subject.Add(g3);

        var expected = new HashSet<string>(StringComparer.Ordinal) { "g1", "g2", "g3" };

        // Exact size
        var arr = new GroupPolicy[3];
        subject.CopyTo(arr, 0);

        arr.Length.Be(3);
        arr.Select(x => x.NameIdentifier).ToHashSet(StringComparer.Ordinal).SetEquals(expected).BeTrue();

        // With offset
        var arr2 = new GroupPolicy[5];
        subject.CopyTo(arr2, 1);

        (arr2[0].NameIdentifier == null).BeTrue(); // default slot untouched
        arr2.Skip(1).Where(x => x.NameIdentifier != null)
            .Select(x => x.NameIdentifier)
            .ToHashSet(StringComparer.Ordinal)
            .SetEquals(expected).BeTrue();
    }

    [Fact]
    public void CopyTo_Throws_On_BadArgs()
    {
        var subject = new GroupCollection();
        subject.Add(new GroupPolicy("g1", new[] { "u1" }));

        Assert.Throws<ArgumentOutOfRangeException>(() => subject.CopyTo(new GroupPolicy[1], -1));
        Assert.Throws<ArgumentException>(() => subject.CopyTo(new GroupPolicy[0], 0));
    }

    [Fact]
    public void Enumerator_Returns_All_Items()
    {
        var subject = new GroupCollection();
        var g1 = new GroupPolicy("g1", new[] { "u1" });
        var g2 = new GroupPolicy("g2", new[] { "u2" });
        var g3 = new GroupPolicy("g3", new[] { "u3" });

        subject.Add(g1);
        subject.Add(g2);
        subject.Add(g3);

        var items = subject.ToList();
        items.Count.Be(3);

        items.Contains(g1).BeTrue();
        items.Contains(g2).BeTrue();
        items.Contains(g3).BeTrue();
    }

    [Fact]
    public void InGroup_Tests()
    {
        var subject = new GroupCollection(new[]
        {
            new GroupPolicy("devs", new[] { "alice", "bob" }),
            new GroupPolicy("ops",  new[] { "carol" }),
        });

        subject.InGroup("devs", "alice").BeTrue();
        subject.InGroup("devs", "eve").BeFalse();

        subject.InGroup("ops", "carol").BeTrue();
        subject.InGroup("ops", "bob").BeFalse();

        subject.InGroup("unknown", "any").BeFalse();
    }

    [Fact]
    public void Equality_SameContent()
    {
        var a = new GroupCollection(new[]
        {
            new GroupPolicy("g1", new[] { "u1", "u2" }),
            new GroupPolicy("g2", new[] { "u3" }),
        });

        var b = new GroupCollection(new[]
        {
            new GroupPolicy("g1", new[] { "u1", "u2" }),
            new GroupPolicy("g2", new[] { "u3" }),
        });

        a.Equals(b).BeTrue();
        (a == b).BeTrue();
        (a != b).BeFalse();

        (a == null).BeFalse();
        (a != null).BeTrue();
    }

    [Fact]
    public void Equality_DifferentContent()
    {
        var a = new GroupCollection(new[]
        {
            new GroupPolicy("g1", new[] { "u1", "u2" }),
        });

        var b = new GroupCollection(new[]
        {
            new GroupPolicy("g1", new[] { "u1" }), // different members
        });

        a.Equals(b).BeFalse();
        (a == b).BeFalse();
        (a != b).BeTrue();
    }

    [Fact]
    public void Add_And_Indexer_Throw_On_Invalid_GroupPolicy()
    {
        var subject = new GroupCollection();

        // default(GroupPolicy) fails validation (empty NameIdentifier)
        Assert.Throws<ArgumentException>(() => subject.Add(default));

        // Indexer set should also validate
        Assert.Throws<ArgumentNullException>(() => subject[""] = new GroupPolicy("", new[] { "u1" }));
    }
}
