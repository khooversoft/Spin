using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph.test.Policy;

public class PrincipalIdentityCollectionTests
{

    [Fact]
    public void PolicyUserGroupCollection_Serialization_RoundTrip()
    {
        // Arrange
        var originalCollection = new PrincipalCollection(new[]
        {
            new PrincipalIdentity("nameIdentifier", "user1", "email@domain.com", false),
            new PrincipalIdentity("nameIdentifier2", "user1", "email2@domain.com", false),
        });

        // Act
        var json = originalCollection.ToJson();
        var deserializedCollection = json.ToObject<PrincipalCollection>();

        // Assert
        (deserializedCollection != default).BeTrue();
        (originalCollection == deserializedCollection).BeTrue();
    }

    [Fact]
    public void PolicyUserGroupCollectionEdit()
    {
        var list = new PrincipalCollection();
        list.Add(new PrincipalIdentity("id1", "nameIdentifier", "user1", "email@domain.com", false));
        list.Count.Be(1);

        list.Add(new PrincipalIdentity("id2", "nameIdentifier2", "user2", "email2@domain.com", false));
        list.Count.Be(2);

        var list2 = new PrincipalCollection()
        {
            new PrincipalIdentity("id1", "nameIdentifier", "user1", "email@domain.com", false),
            new PrincipalIdentity("id2", "nameIdentifier2", "user2", "email2@domain.com", false),
        };

        (list == list2).BeTrue();
    }

    [Fact]
    public void Add_And_Retrieve_ById_And_ByNameIdentifier()
    {
        var list = new PrincipalCollection();
        var p = new PrincipalIdentity("nid-1", "userA", "userA@domain.com", false);

        list.Add(p);

        list.Contains(p.PrincipalId).BeTrue();
        list.Contains(p).BeTrue();

        list.TryGetValue(p.PrincipalId, out var byId).BeTrue();
        (byId == p).BeTrue();

        list.TryGetByNameIdentifier(p.NameIdentifier, out var byName).BeTrue();
        (byName == p).BeTrue();
    }

    [Fact]
    public void Replace_SamePrincipalId_NewNameIdentifier_UpdatesNameIndex()
    {
        var list = new PrincipalCollection();

        var p1 = new PrincipalIdentity("user:fixed-1", "nid-old", "userA", "userA@domain.com", false);
        list.Add(p1);

        list.TryGetByNameIdentifier("nid-old", out _).BeTrue();

        // Replace with same PrincipalId but different NameIdentifier
        var p2 = new PrincipalIdentity("user:fixed-1", "nid-new", "userA", "userA@domain.com", false);
        list.Add(p2);

        list.Count.Be(1);

        (!list.TryGetByNameIdentifier("nid-old", out _)).BeTrue();
        list.TryGetByNameIdentifier("nid-new", out var current).BeTrue();
        (current == p2).BeTrue();
    }

    [Fact]
    public void Remove_ById_RemovesNameIndex_And_ReturnsExpected()
    {
        var list = new PrincipalCollection();

        var p = new PrincipalIdentity("user:fixed-2", "nid-x", "userB", "userB@domain.com", false);
        list.Add(p);

        list.Contains(p.PrincipalId).BeTrue();
        list.TryGetByNameIdentifier("nid-x", out _).BeTrue();

        var removed1 = list.Remove(p.PrincipalId);
        removed1.BeTrue();

        (!list.Contains(p.PrincipalId)).BeTrue();
        (!list.TryGetByNameIdentifier("nid-x", out _)).BeTrue();

        var removed2 = list.Remove(p.PrincipalId);
        (!removed2).BeTrue();
    }

    [Fact]
    public void Contains_PrincipalIdentity_UsesRecordValueEquality()
    {
        var list = new PrincipalCollection();

        var original = new PrincipalIdentity("user:fixed-3", "nid-y", "userC", "userC@domain.com", true);
        list.Add(original);

        // Same values but different instance; record equality should consider them equal
        var equalByValue = new PrincipalIdentity(original.PrincipalId, original.NameIdentifier, original.UserName, original.Email, original.EmailConfirmed);

        list.Contains(equalByValue).BeTrue();
    }

    [Fact]
    public void CopyTo_CopiesSnapshot_And_ThrowsOnInvalidArgs()
    {
        var list = new PrincipalCollection
        {
            new PrincipalIdentity("user:fixed-4", "nid-1", "user1", "user1@domain.com", false),
            new PrincipalIdentity("user:fixed-5", "nid-2", "user2", "user2@domain.com", false),
        };

        // Successful copy
        var arr = new PrincipalIdentity[4];
        list.CopyTo(arr, 1);

        var copied = arr.Where((x, i) => i >= 1 && x != null).Take(list.Count).ToArray();
        copied.Length.Be(list.Count);

        var sourceIds = list.Select(x => x.PrincipalId).ToHashSet();
        var destIds = copied.Select(x => x.PrincipalId).ToHashSet();
        (sourceIds.SetEquals(destIds)).BeTrue();

        // Throws on negative arrayIndex
        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => list.CopyTo(new PrincipalIdentity[1], -1));

        // Throws when destination too small
        Assert.ThrowsAny<ArgumentException>(() => list.CopyTo(new PrincipalIdentity[1], 0));
    }

    [Fact]
    public void Clear_ResetsState()
    {
        var list = new PrincipalCollection
        {
            new PrincipalIdentity("user:fixed-6", "nid-a", "userA", "userA@domain.com", false),
            new PrincipalIdentity("user:fixed-7", "nid-b", "userB", "userB@domain.com", false),
        };

        list.Count.Be(2);
        list.Clear();
        list.Count.Be(0);

        (!list.TryGetValue("user:fixed-6", out _)).BeTrue();
        (!list.TryGetByNameIdentifier("nid-a", out _)).BeTrue();
    }

    [Fact]
    public void IsReadOnly_IsFalse()
    {
        var list = new PrincipalCollection();
        list.IsReadOnly.BeFalse();
    }

    [Fact]
    public void Constructor_Throws_OnInvalidPrincipal()
    {
        // Invalid email format should fail validation inside collection constructor
        var invalid = new PrincipalIdentity("nid-bad", "userX", "not-an-email", false);

        Assert.ThrowsAny<Exception>(() => new PrincipalCollection(new[] { invalid }));
    }

    [Fact]
    public void Add_Throws_OnInvalidPrincipal()
    {
        var list = new PrincipalCollection();
        var invalid = new PrincipalIdentity("nid-bad", "userX", "not-an-email", false);

        Assert.ThrowsAny<Exception>(() => list.Add(invalid));
    }
}
