using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Data.IndexedCollection;

public class IndexedCollectionTests
{
    private record TestRec
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    [Fact]
    public void EmptyCollection()
    {
        var collection = new IndexedCollection<int, TestRec>(x => x.Id);
        collection.Count.Be(0);
        collection.ToArray().Length.Be(0);
    }

    [Fact]
    public void SingleRecord()
    {
        var collection = new IndexedCollection<int, TestRec>(x => x.Id);
        var item = new TestRec { Id = 1, Name = "Test" };

        collection.TryAdd(item).BeTrue();
        collection.Count.Be(1);
        collection.ToArray().Length.Be(1);
    }

    [Fact]
    public void TryAdd_DuplicateKey_Fails()
    {
        var c = new IndexedCollection<int, TestRec>(x => x.Id);
        c.TryAdd(new TestRec { Id = 1, Name = "A" }).BeTrue();
        c.TryAdd(new TestRec { Id = 1, Name = "B" }).BeFalse();
        c.Count.Be(1);
        c.ContainsKey(1).BeTrue();
    }

    [Fact]
    public void GetOrAdd_ReturnsExisting_And_DoesNotTouchSecondary_WhenExists()
    {
        var c = new IndexedCollection<int, TestRec>(x => x.Id);

        // Unique secondary index by Name (case-insensitive)
        var unique = c.SecondaryIndexes.CreateUniqueIndex("byName", x => x.Name, StringComparer.OrdinalIgnoreCase).BeOk().Return();

        var first = new TestRec { Id = 1, Name = "Alpha" };
        c.TryAdd(first).BeTrue();

        // Same primary key, different Name
        var second = new TestRec { Id = 1, Name = "Beta" };
        var returned = c.GetOrAdd(second);
        returned.Name.Be("Alpha"); // existing returned

        // Secondary index should still reflect original item only
        unique.TryGetValue("ALPHA", out var v1).BeTrue();
        v1!.Id.Be(1);

        unique.TryGetValue("beta", out var v2).BeFalse();
    }

    [Fact]
    public void ContainsKey_TryGetValue_Keys_Values_Enumerate()
    {
        var c = new IndexedCollection<int, TestRec>(x => x.Id);
        var a = new TestRec { Id = 1, Name = "A" };
        var b = new TestRec { Id = 2, Name = "B" };
        c.TryAdd(a).BeTrue();
        c.TryAdd(b).BeTrue();

        c.ContainsKey(1).BeTrue();
        c.TryGetValue(2, out var vb).BeTrue();
        vb!.Name.Be("B");

        c.Keys.OrderBy(x => x).SequenceEqual(new[] { 1, 2 }).BeTrue();
        c.Values.Select(x => x.Id).OrderBy(x => x).SequenceEqual(new[] { 1, 2 }).BeTrue();

        c.Count.Be(2);
        c.ToArray().Length.Be(2);
    }

    [Fact]
    public void Remove_ByKey_And_ByValue_UpdatesSecondary()
    {
        var c = new IndexedCollection<int, TestRec>(x => x.Id);

        // Non-unique secondary by first letter of Name
        var nonUnique = c.SecondaryIndexes.CreateNonUniqueIndex("byFirst", x => x.Name.Substring(0, 1), StringComparer.OrdinalIgnoreCase).BeOk().Return();

        var ann = new TestRec { Id = 1, Name = "Ann" };
        var alice = new TestRec { Id = 2, Name = "Alice" };
        var bob = new TestRec { Id = 3, Name = "Bob" };

        c.TryAdd(ann).BeTrue();
        c.TryAdd(alice).BeTrue();
        c.TryAdd(bob).BeTrue();

        nonUnique.TryGetValue("A", out var aList).BeTrue();
        aList!.Count.Be(2);

        // Remove by key
        c.TryRemove(1, out var _).BeTrue();
        nonUnique.TryGetValue("A", out var aList2).BeTrue();
        aList2!.Select(x => x.Id).SequenceEqual(new[] { 2 }).BeTrue();

        // Remove by value
        c.TryRemove(alice, out var _).BeTrue();
        nonUnique.TryGetValue("A", out var aList3).BeFalse();

        // B still present
        nonUnique.TryGetValue("b", out var bList).BeTrue();
        bList!.Count.Be(1);
        bList[0].Id.Be(3);
    }

    [Fact]
    public void Clear_Empties_Primary_And_Secondary()
    {
        var c = new IndexedCollection<int, TestRec>(x => x.Id);
        var unique = c.SecondaryIndexes.CreateUniqueIndex("byName", x => x.Name, StringComparer.OrdinalIgnoreCase).BeOk().Return();

        c.TryAdd(new TestRec { Id = 1, Name = "Alpha" }).BeTrue();
        c.TryAdd(new TestRec { Id = 2, Name = "Beta" }).BeTrue();
        c.Count.Be(2);

        c.Clear();

        c.Count.Be(0);
        c.ToArray().Length.Be(0);
        unique.TryGetValue("alpha", out var _).BeFalse();
        unique.TryGetValue("beta", out var _).BeFalse();
    }

    [Fact]
    public void Indexer_Get_Set_And_KeyMismatch_Throws()
    {
        var c = new IndexedCollection<int, TestRec>(x => x.Id);

        // Set via indexer with matching key
        c[42] = new TestRec { Id = 42, Name = "X" };
        c.Count.Be(1);
        c[42].Name.Be("X");

        // Get missing key throws
        Assert.Throws<KeyNotFoundException>(() => { var _ = c[999]; });

        // Set with mismatched key throws
        Assert.Throws<ArgumentException>(() => c[41] = new TestRec { Id = 42, Name = "Mismatch" });
    }

    [Fact]
    public void TryUpdate_Updates_Primary_And_Adds_New_Secondary_Key()
    {
        var c = new IndexedCollection<int, TestRec>(x => x.Id);
        var unique = c.SecondaryIndexes.CreateUniqueIndex("byName", x => x.Name, StringComparer.OrdinalIgnoreCase).BeOk().Return();

        var current = new TestRec { Id = 1, Name = "Alpha" };
        c.TryAdd(current).BeTrue();

        var updated = new TestRec { Id = 1, Name = "Beta" };
        c.TryUpdate(updated, current).BeTrue();

        c[1].Name.Be("Beta");
        unique.TryGetValue("beta", out var v).BeTrue();
        v!.Id.Be(1);

        // Note: old secondary key "Alpha" is not removed by current implementation.
        // If design changes to remove it, update this test accordingly.
    }

    [Fact]
    public void PrimaryKeyComparer_CaseInsensitive_Works()
    {
        // Use Name as primary key to exercise string comparer
        var c = new IndexedCollection<string, TestRec>(x => x.Name, StringComparer.OrdinalIgnoreCase);

        var a = new TestRec { Id = 1, Name = "Alpha" };
        c.TryAdd(a).BeTrue();

        c.ContainsKey("ALPHA").BeTrue();
        c.TryGetValue("alpha", out var v).BeTrue();
        v!.Id.Be(1);

        // Indexer get using different casing works
        c["ALPHA"].Id.Be(1);
    }

    [Fact]
    public void Multiple_NonUniqueIndexes_Add_Remove_Clear()
    {
        var c = new IndexedCollection<int, TestRec>(x => x.Id);

        var byFirst = c.SecondaryIndexes
            .CreateNonUniqueIndex("byFirst", x => x.Name.Substring(0, 1), StringComparer.OrdinalIgnoreCase)
            .BeOk().Return();

        var byLen = c.SecondaryIndexes
            .CreateNonUniqueIndex("byLen", x => x.Name.Length)
            .BeOk().Return();

        var ann = new TestRec { Id = 1, Name = "Ann" };   // first=A, len=3
        var alice = new TestRec { Id = 2, Name = "Alice" }; // first=A, len=5
        var bob = new TestRec { Id = 3, Name = "Bob" };   // first=B, len=3

        c.TryAdd(ann).BeTrue();
        c.TryAdd(alice).BeTrue();
        c.TryAdd(bob).BeTrue();

        // byFirst
        byFirst.TryGetValue("A", out var aList).BeTrue();
        aList!.Select(x => x.Id).OrderBy(x => x).SequenceEqual(new[] { 1, 2 }).BeTrue();
        byFirst.TryGetValue("B", out var bList).BeTrue();
        bList!.Select(x => x.Id).SequenceEqual(new[] { 3 }).BeTrue();

        // byLen
        byLen.TryGetValue(3, out var len3).BeTrue();
        len3!.Select(x => x.Id).OrderBy(x => x).SequenceEqual(new[] { 1, 3 }).BeTrue();
        byLen.TryGetValue(5, out var len5).BeTrue();
        len5!.Select(x => x.Id).SequenceEqual(new[] { 2 }).BeTrue();

        // Remove by key
        c.TryRemove(1, out _).BeTrue();

        byFirst.TryGetValue("A", out var aList2).BeTrue();
        aList2!.Select(x => x.Id).SequenceEqual(new[] { 2 }).BeTrue();
        byLen.TryGetValue(3, out var len3b).BeTrue();
        len3b!.Select(x => x.Id).SequenceEqual(new[] { 3 }).BeTrue();

        // Remove by value
        c.TryRemove(bob, out _).BeTrue();

        byFirst.TryGetValue("B", out var bList2).BeFalse();
        byLen.TryGetValue(3, out var len3c).BeFalse();

        // Clear remaining
        c.Clear();
        byFirst.TryGetValue("A", out var _).BeFalse();
        byLen.TryGetValue(5, out var _).BeFalse();
    }

    [Fact]
    public void Multiple_NonUniqueIndexes_TryUpdate_Adds_New_Keys_To_All_Indexes()
    {
        var c = new IndexedCollection<int, TestRec>(x => x.Id);

        var byFirst = c.SecondaryIndexes
            .CreateNonUniqueIndex("byFirst", x => x.Name.Substring(0, 1), StringComparer.OrdinalIgnoreCase)
            .BeOk().Return();

        var byLast = c.SecondaryIndexes
            .CreateNonUniqueIndex("byLast", x => x.Name.Substring(x.Name.Length - 1, 1), StringComparer.OrdinalIgnoreCase)
            .BeOk().Return();

        var current = new TestRec { Id = 1, Name = "Ann" };   // first=A, last=n
        c.TryAdd(current).BeTrue();

        var updated = new TestRec { Id = 1, Name = "Beta" };  // first=B, last=a
        c.TryUpdate(updated, current).BeTrue();

        // Both indexes should now have entries for the new keys
        byFirst.TryGetValue("b", out var listFirst).BeTrue();
        listFirst!.Any(x => x.Id == 1).BeTrue();

        byLast.TryGetValue("A", out var listLast).BeTrue();
        listLast!.Any(x => x.Id == 1).BeTrue();

        // Note: old keys may still exist for the old item per current implementation.
        // This test intentionally does not assert their absence.
    }
}
