using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Data.IndexedCollection;

public class SecondaryIndexCollectionTests
{
    private record TestRecord
    {
        public string Id { get; init; } = null!;
        public string Name { get; init; } = null!;
    }

    [Fact]
    public void Empty()
    {
        var c = new SecondaryIndexCollection<string, TestRecord>();
        c.Count.Be(0);
        c.Providers.Count().Be(0);
        c.GetIndex("nonexistent").BeNotFound();
    }

    [Fact]
    public void UniqueIndex_Create_Lookup_Update_Remove()
    {
        var c = new SecondaryIndexCollection<string, TestRecord>();

        // Create unique index by Name, case-insensitive keys
        var unique = c.CreateUniqueIndex("byName", r => r.Name, StringComparer.OrdinalIgnoreCase).BeOk().Return();
        c.Count.Be(1);

        // Access the provider to Set/Remove
        var provider = c.GetIndex("ByName").BeOk().Return();

        var a1 = new TestRecord { Id = "1", Name = "Alice" };
        provider.Set(a1);

        unique.TryGetValue("alice", out var read1).BeTrue();
        read1!.Id.Be("1");

        // Update (same key), should overwrite
        var a2 = new TestRecord { Id = "1b", Name = "Alice" };
        provider.Set(a2);

        unique.TryGetValue("ALICE", out var read2).BeTrue();
        read2!.Id.Be("1b");

        // Remove by computing same key (using old instance is fine)
        provider.Remove(a1);
        unique.TryGetValue("Alice", out var read3).BeFalse();
    }

    [Fact]
    public void NonUniqueIndex_Create_Lookup_Remove_Clear()
    {
        var c = new SecondaryIndexCollection<string, TestRecord>();

        // Create non-unique by first letter of Name
        var nonUnique = c.CreateNonUniqueIndex("byFirst", r => r.Name.Substring(0, 1), StringComparer.OrdinalIgnoreCase).BeOk().Return();
        var provider = c.GetIndex("byfirst").BeOk().Return();

        var ann = new TestRecord { Id = "A1", Name = "Ann" };
        var alice = new TestRecord { Id = "A2", Name = "Alice" };
        var bob = new TestRecord { Id = "B1", Name = "Bob" };

        provider.Set(ann);
        provider.Set(alice);
        provider.Set(bob);

        nonUnique.TryGetValue("a", out var aList).BeTrue();
        aList!.Count.Be(2);
        aList.Select(x => x.Id).OrderBy(x => x).SequenceEqual(new[] { "A1", "A2" }).BeTrue();

        nonUnique.TryGetValue("B", out var bList).BeTrue();
        bList!.Count.Be(1);
        bList[0].Id.Be("B1");

        // Remove one item under "A"
        provider.Remove(ann);
        nonUnique.TryGetValue("A", out var aList2).BeTrue();
        aList2!.Count.Be(1);
        aList2[0].Id.Be("A2");

        // Remove last item under "A" -> then TryGetValue should fail
        provider.Remove(alice);
        nonUnique.TryGetValue("A", out var aList3).BeFalse();

        // Clear and verify
        provider.Clear();
        nonUnique.TryGetValue("B", out var bList2).BeFalse();
    }

    [Fact]
    public void DuplicateIndexName_Conflict()
    {
        var c = new SecondaryIndexCollection<string, TestRecord>();

        c.CreateUniqueIndex("dup", r => r.Id).BeOk();
        c.CreateNonUniqueIndex("dup", r => r.Name).Be(StatusCode.Conflict);
        c.Count.Be(1);
        c.Providers.Count().Be(1);
    }

    [Fact]
    public void RemoveIndex_Ok_Then_NotFound()
    {
        var c = new SecondaryIndexCollection<string, TestRecord>();
        c.CreateUniqueIndex("U", r => r.Id).BeOk();
        c.CreateNonUniqueIndex("N", r => r.Name).BeOk();

        c.Count.Be(2);
        c.RemoveIndex("u").BeOk();
        c.Count.Be(1);
        c.GetIndex("U").BeNotFound();

        c.RemoveIndex("u").Be(StatusCode.NotFound);
        c.RemoveIndex("N").BeOk();
        c.Count.Be(0);
    }

    [Fact]
    public void GetIndex_IsCaseInsensitive_And_ProvidersEnumerate()
    {
        var c = new SecondaryIndexCollection<string, TestRecord>();
        c.CreateUniqueIndex("byId", r => r.Id).BeOk();
        c.CreateNonUniqueIndex("byName", r => r.Name).BeOk();

        c.GetIndex("BYID").BeOk();
        c.GetIndex("ByName").BeOk();

        c.Count.Be(2);
        c.Providers.Count().Be(2);

        var names = c.Select(kv => kv.Key).ToArray();
        new[] { "byId", "byName" }.All(x => names.Contains(x, StringComparer.OrdinalIgnoreCase)).BeTrue();
    }

    [Fact]
    public void UniqueAndNonUnique_CustomComparers_Work()
    {
        var c = new SecondaryIndexCollection<string, TestRecord>();

        var u = c.CreateUniqueIndex("u", r => r.Name, StringComparer.OrdinalIgnoreCase).BeOk().Return();
        var n = c.CreateNonUniqueIndex("n", r => r.Name, StringComparer.OrdinalIgnoreCase).BeOk().Return();
        var up = c.GetIndex("u").BeOk().Return();
        var np = c.GetIndex("n").BeOk().Return();

        var r1 = new TestRecord { Id = "1", Name = "User" };
        var r2 = new TestRecord { Id = "2", Name = "user" }; // same by comparer

        up.Set(r1);
        u.TryGetValue("USER", out var urec).BeTrue();
        urec!.Id.Be("1");

        // Unique: overwrite same key (per comparer)
        up.Set(r2);
        u.TryGetValue("user", out var urec2).BeTrue();
        urec2!.Id.Be("2");

        // Non-unique: both should be present under the same logical key
        np.Set(r1);
        np.Set(r2);
        n.TryGetValue("UsEr", out var list).BeTrue();
        list!.Select(x => x.Id).OrderBy(x => x).SequenceEqual(new[] { "1", "2" }).BeTrue();
    }
}
