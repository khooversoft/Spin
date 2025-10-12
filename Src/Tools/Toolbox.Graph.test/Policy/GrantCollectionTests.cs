using Toolbox.Tools;

namespace Toolbox.Graph;

public class GrantCollectionTests
{
    [Fact]
    public void EmptyCollection_Defaults()
    {
        var subject = new GrantCollection();

        subject.Count.Be(0);
        subject.IsReadOnly.BeFalse();

        subject.TryGetValue("does-not-exist", out var _).BeFalse();
        subject.Contains(new GrantPolicy("n1", RolePolicy.Reader | RolePolicy.NameIdentifier, "u1")).BeFalse();

        subject.ToList().Count.Be(0);
    }

    [Fact]
    public void Add_Contains_TryGetValue()
    {
        var subject = new GrantCollection();
        var p = new GrantPolicy("n1", RolePolicy.Reader | RolePolicy.NameIdentifier, "u1");

        subject.Add(p);

        subject.Count.Be(1);
        subject.Contains(p).BeTrue();

        subject.TryGetValue("n1", out var list).BeTrue();
        list.NotNull();
        list.Count.Be(1);
        list.Contains(p).BeTrue();
    }

    [Fact]
    public void Add_Groups_By_NameIdentifier()
    {
        var subject = new GrantCollection();

        var p1 = new GrantPolicy("n1", RolePolicy.Reader | RolePolicy.NameIdentifier, "u1");
        var p2 = new GrantPolicy("n1", RolePolicy.Contributor | RolePolicy.NameIdentifier, "u2");
        var p3 = new GrantPolicy("n2", RolePolicy.Owner | RolePolicy.NameIdentifier, "u3");

        subject.Add(p1);
        subject.Add(p2);
        subject.Add(p3);

        subject.Count.Be(3);

        subject.TryGetValue("n1", out var g1).BeTrue();
        g1.NotNull();
        g1.Count.Be(2);
        g1.Contains(p1).BeTrue();
        g1.Contains(p2).BeTrue();

        subject.TryGetValue("n2", out var g2).BeTrue();
        g2.NotNull();
        g2.Count.Be(1);
        g2.Contains(p3).BeTrue();
    }

    [Fact]
    public void Remove_Items_Updates_Groups()
    {
        var subject = new GrantCollection();

        var p1 = new GrantPolicy("n1", RolePolicy.Reader | RolePolicy.NameIdentifier, "u1");
        var p2 = new GrantPolicy("n2", RolePolicy.Contributor | RolePolicy.NameIdentifier, "u2");
        var p3 = new GrantPolicy("n2", RolePolicy.Owner | RolePolicy.NameIdentifier, "u3");

        subject.Add(p1);
        subject.Add(p2);
        subject.Add(p3);

        subject.Count.Be(3);

        subject.Remove(p1).BeTrue();
        subject.Count.Be(2);
        subject.TryGetValue("n1", out var _).BeFalse();

        subject.Remove(p2).BeTrue();
        subject.Count.Be(1);
        subject.TryGetValue("n2", out var g2).BeTrue();
        g2.Count.Be(1);
        g2.Contains(p3).BeTrue();

        subject.Remove(p3).BeTrue();
        subject.Count.Be(0);
        subject.TryGetValue("n2", out var _).BeFalse();

        subject.Remove(p3).BeFalse();
    }

    [Fact]
    public void CopyTo_Basic_And_Offset()
    {
        var subject = new GrantCollection();

        var p1 = new GrantPolicy("n1", RolePolicy.Reader | RolePolicy.NameIdentifier, "u1");
        var p2 = new GrantPolicy("n1", RolePolicy.Contributor | RolePolicy.NameIdentifier, "u2");
        var p3 = new GrantPolicy("n2", RolePolicy.Owner | RolePolicy.NameIdentifier, "u3");

        subject.Add(p1);
        subject.Add(p2);
        subject.Add(p3);

        var expected = new HashSet<string>(StringComparer.Ordinal)
        {
            p1.Encode(), p2.Encode(), p3.Encode()
        };

        // Exact size
        var arr = new GrantPolicy[3];
        subject.CopyTo(arr, 0);

        var actual = arr
            .Where(x => x.NameIdentifier != null)
            .Select(x => x.Encode())
            .ToHashSet(StringComparer.Ordinal);

        actual.SetEquals(expected).BeTrue();

        // With offset
        var arr2 = new GrantPolicy[5];
        subject.CopyTo(arr2, 1);

        var actual2 = arr2
            .Skip(1)
            .Where(x => x.NameIdentifier != null)
            .Select(x => x.Encode())
            .ToHashSet(StringComparer.Ordinal);

        actual2.SetEquals(expected).BeTrue();
        (arr2[0].NameIdentifier == null).BeTrue();
    }

    [Fact]
    public void CopyTo_Throws_On_BadArgs()
    {
        var subject = new GrantCollection();
        subject.Add(new GrantPolicy("n1", RolePolicy.Reader | RolePolicy.NameIdentifier, "u1"));

        Assert.Throws<ArgumentOutOfRangeException>(() => subject.CopyTo(new GrantPolicy[1], -1));
        Assert.Throws<ArgumentException>(() => subject.CopyTo(new GrantPolicy[0], 0));
    }

    [Fact]
    public void Enumeration_Returns_All_Items()
    {
        var subject = new GrantCollection();

        var p1 = new GrantPolicy("n1", RolePolicy.Reader | RolePolicy.NameIdentifier, "u1");
        var p2 = new GrantPolicy("n1", RolePolicy.Contributor | RolePolicy.NameIdentifier, "u2");
        var p3 = new GrantPolicy("n2", RolePolicy.Owner | RolePolicy.NameIdentifier, "u3");

        subject.Add(p1);
        subject.Add(p2);
        subject.Add(p3);

        var items = subject.ToList();
        items.Count.Be(subject.Count);

        items.Contains(p1).BeTrue();
        items.Contains(p2).BeTrue();
        items.Contains(p3).BeTrue();
    }

    [Fact]
    public void Equality_SameContent()
    {
        var p1 = new GrantPolicy("n1", RolePolicy.Reader | RolePolicy.NameIdentifier, "u1");
        var p2 = new GrantPolicy("n2", RolePolicy.Contributor | RolePolicy.NameIdentifier, "u2");

        var a = new GrantCollection(new[] { p1, p2 });
        var b = new GrantCollection(new[] { p1, p2 });

        a.Equals(b).BeTrue();
        (a == b).BeTrue();
        (a != b).BeFalse();

        (a == null).BeFalse();
        (a != null).BeTrue();
    }

    [Fact]
    public void Equality_DifferentOrderWithinGroup_NotEqual()
    {
        var p1 = new GrantPolicy("n1", RolePolicy.Reader | RolePolicy.NameIdentifier, "u1");
        var p2 = new GrantPolicy("n1", RolePolicy.Contributor | RolePolicy.NameIdentifier, "u2");

        var a = new GrantCollection(new[] { p1, p2 });
        var b = new GrantCollection(new[] { p2, p1 }); // reversed order within same key

        a.Equals(b).BeFalse();
        (a == b).BeFalse();
        (a != b).BeTrue();
    }

    [Fact]
    public void Clear_Empties_Collection()
    {
        var subject = new GrantCollection();

        subject.Add(new GrantPolicy("n1", RolePolicy.Reader | RolePolicy.NameIdentifier, "u1"));
        subject.Add(new GrantPolicy("n2", RolePolicy.Contributor | RolePolicy.NameIdentifier, "u2"));

        subject.Count.Be(2);

        subject.Clear();
        subject.Count.Be(0);
        subject.ToList().Count.Be(0);
    }
}
