using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph.test.Policy;

public class GroupTests
{
    [Fact]
    public void PolicyUserGroup_Serialization_RoundTrip()
    {
        // Arrange
        var originalUserGroup = new GroupPolicy("group1", new[]
        {
            "user1",
            "user2",
            "user3"
        });

        // Act
        var json = originalUserGroup.ToJson();
        var deserializedUserGroup = json.ToObject<GroupPolicy>();

        // Assert
        (deserializedUserGroup != default).BeTrue();
        (originalUserGroup == deserializedUserGroup).BeTrue();
    }

    [Fact]
    public void PolicyUserGroupCollection_Serialization_RoundTrip()
    {
        // Arrange
        var originalCollection = new GroupCollection(new[]
        {
            new GroupPolicy("group1", new[] { "user1", "user2" }),
            new GroupPolicy("group2", new[] { "user3", "user4" }),
        });

        // Act
        var json = originalCollection.ToJson();
        var deserializedCollection = json.ToObject<GroupCollection>();

        // Assert
        (deserializedCollection != default).BeTrue();
        (originalCollection == deserializedCollection).BeTrue();
    }

    [Fact]
    public void EqualNotEqual()
    {
        var v1 = new GroupPolicy("group1", new[] { "user1", "user2" });
        var v2 = new GroupPolicy("group1", new[] { "user1", "user2" });
        (v1 == v2).BeTrue();
        (v1 != v2).BeFalse();

        var v3 = new GroupPolicy("group1");
        var v4 = new GroupPolicy("group1", []);
        (v3 == v4).BeTrue();

        var v5 = new GroupPolicy("group1", new[] { "user1" });
        (v1 == v5).BeFalse();
        (v1 != v5).BeTrue();

        var v6 = new GroupPolicy("group1x", new[] { "user1", "user2" });
        (v1 == v6).BeFalse();
    }

    // ============ NEW TESTS BELOW ============

    [Fact]
    public void GroupPolicy_Constructor_SingleParameter_ShouldCreateWithEmptyMembers()
    {
        // Act
        var group = new GroupPolicy("group1");

        // Assert
        group.NameIdentifier.Be("group1");
        (group.Members.Count == 0).BeTrue();
    }

    [Fact]
    public void GroupPolicy_Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var group = new GroupPolicy("group1", new[] { "user1" });

        // Act & Assert
        group.Equals(null).BeFalse();
    }

    [Fact]
    public void GroupPolicy_Equals_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        var group = new GroupPolicy("group1", new[] { "user1" });

        // Act & Assert
        group.Equals("not a group").BeFalse();
    }

    [Fact]
    public void GroupPolicy_Members_OrderMatters_ShouldNotBeEqual()
    {
        // Arrange
        var group1 = new GroupPolicy("group1", new[] { "user1", "user2" });
        var group2 = new GroupPolicy("group1", new[] { "user2", "user1" });

        // Act & Assert
        (group1 == group2).BeFalse();
    }

    [Fact]
    public void GroupPolicy_Validate_WithEmptyNameIdentifier_ShouldFail()
    {
        // Arrange & Act
        Action result = () => new GroupPolicy("", new[] { "user1" });

        // Assert
        Verify.Throw<ArgumentException>(result);
    }

    [Fact]
    public void GroupPolicy_Validate_ValidGroup_ShouldPass()
    {
        // Arrange
        var group = new GroupPolicy("group1", new[] { "user1" });

        // Act
        var result = group.Validate();

        // Assert
        result.BeOk();
    }

    // ============ GroupCollection Tests ============

    [Fact]
    public void GroupCollection_Add_ShouldAddGroup()
    {
        // Arrange
        var collection = new GroupCollection();
        var group = new GroupPolicy("group1", new[] { "user1", "user2" });

        // Act
        collection.Add(group);

        // Assert
        (collection.Count == 1).BeTrue();
        collection.Contains("group1").BeTrue();
    }

    [Fact]
    public void GroupCollection_Add_DuplicateNameIdentifier_ShouldReplace()
    {
        // Arrange
        var collection = new GroupCollection();
        var group1 = new GroupPolicy("group1", new[] { "user1" });
        var group2 = new GroupPolicy("group1", new[] { "user2", "user3" });

        // Act
        collection.Add(group1);
        collection.Add(group2);

        // Assert
        (collection.Count == 1).BeTrue();
        collection.TryGetGroup("group1", out var result).BeTrue();
        (result.Members.Count == 2).BeTrue();
    }

    [Fact]
    public void GroupCollection_Remove_ShouldRemoveGroup()
    {
        // Arrange
        var group = new GroupPolicy("group1", new[] { "user1" });
        var collection = new GroupCollection(new[] { group });

        // Act
        var removed = collection.Remove(group);

        // Assert
        removed.BeTrue();
        (collection.Count == 0).BeTrue();
    }

    [Fact]
    public void GroupCollection_Clear_ShouldRemoveAllGroups()
    {
        // Arrange
        var collection = new GroupCollection(new[]
        {
            new GroupPolicy("group1", new[] { "user1" }),
            new GroupPolicy("group2", new[] { "user2" })
        });

        // Act
        collection.Clear();

        // Assert
        (collection.Count == 0).BeTrue();
    }

    [Fact]
    public void GroupCollection_Indexer_Get_ShouldReturnGroup()
    {
        // Arrange
        var group = new GroupPolicy("group1", new[] { "user1", "user2" });
        var collection = new GroupCollection(new[] { group });

        // Act
        var result = collection["group1"];

        // Assert
        (result == group).BeTrue();
    }

    [Fact]
    public void GroupCollection_Indexer_Set_ShouldUpdateGroup()
    {
        // Arrange
        var collection = new GroupCollection();
        var group = new GroupPolicy("group1", new[] { "user1", "user2" });

        // Act
        collection["group1"] = group;

        // Assert
        (collection.Count == 1).BeTrue();
        (collection["group1"] == group).BeTrue();
    }

    [Fact]
    public void GroupCollection_Indexer_Set_MismatchedKey_ShouldThrow()
    {
        // Arrange
        var collection = new GroupCollection();
        var group = new GroupPolicy("group1", new[] { "user1" });

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => collection["wrongKey"] = group);
        ex.Message.Contains("does not match indexer key").BeTrue();
    }

    [Fact]
    public void GroupCollection_Contains_WithGroupPolicy_ShouldReturnTrue()
    {
        // Arrange
        var group = new GroupPolicy("group1", new[] { "user1" });
        var collection = new GroupCollection(new[] { group });

        // Act & Assert
        collection.Contains(group).BeTrue();
    }

    [Fact]
    public void GroupCollection_Contains_WithString_ShouldReturnTrue()
    {
        // Arrange
        var group = new GroupPolicy("group1", new[] { "user1" });
        var collection = new GroupCollection(new[] { group });

        // Act & Assert
        collection.Contains("group1").BeTrue();
        collection.Contains("group2").BeFalse();
    }

    [Fact]
    public void GroupCollection_TryGetGroup_Exists_ShouldReturnTrue()
    {
        // Arrange
        var group = new GroupPolicy("group1", new[] { "user1", "user2" });
        var collection = new GroupCollection(new[] { group });

        // Act
        var found = collection.TryGetGroup("group1", out var result);

        // Assert
        found.BeTrue();
        (result == group).BeTrue();
    }

    [Fact]
    public void GroupCollection_TryGetGroup_NotExists_ShouldReturnFalse()
    {
        // Arrange
        var collection = new GroupCollection();

        // Act
        var found = collection.TryGetGroup("nonexistent", out var result);

        // Assert
        found.BeFalse();
        (result == default).BeTrue();
    }

    [Fact]
    public void GroupCollection_InGroup_UserExists_ShouldReturnTrue()
    {
        // Arrange
        var group = new GroupPolicy("group1", new[] { "user1", "user2", "user3" });
        var collection = new GroupCollection(new[] { group });

        // Act & Assert
        collection.InGroup("group1", "user2").BeTrue();
        collection.InGroup("group1", "user4").BeFalse();
    }

    [Fact]
    public void GroupCollection_InGroup_GroupNotExists_ShouldReturnFalse()
    {
        // Arrange
        var collection = new GroupCollection();

        // Act & Assert
        collection.InGroup("nonexistent", "user1").BeFalse();
    }

    [Fact]
    public void GroupCollection_CopyTo_ShouldCopyElements()
    {
        // Arrange
        var groups = new[]
        {
            new GroupPolicy("group1", new[] { "user1" }),
            new GroupPolicy("group2", new[] { "user2" })
        };
        var collection = new GroupCollection(groups);
        var array = new GroupPolicy[3];

        // Act
        collection.CopyTo(array, 1);

        // Assert
        (array[0] == default).BeTrue();
        collection.Contains(array[1]).BeTrue();
        collection.Contains(array[2]).BeTrue();
    }

    [Fact]
    public void GroupCollection_CopyTo_NegativeIndex_ShouldThrow()
    {
        // Arrange
        var collection = new GroupCollection();
        var array = new GroupPolicy[5];

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => collection.CopyTo(array, -1));
    }

    [Fact]
    public void GroupCollection_CopyTo_ArrayTooSmall_ShouldThrow()
    {
        // Arrange
        var groups = new[]
        {
            new GroupPolicy("group1", new[] { "user1" }),
            new GroupPolicy("group2", new[] { "user2" })
        };
        var collection = new GroupCollection(groups);
        var array = new GroupPolicy[2];

        // Act & Assert
        Assert.Throws<ArgumentException>(() => collection.CopyTo(array, 1));
    }

    [Fact]
    public void GroupCollection_GetEnumerator_ShouldEnumerateAllGroups()
    {
        // Arrange
        var groups = new[]
        {
            new GroupPolicy("group1", new[] { "user1" }),
            new GroupPolicy("group2", new[] { "user2" }),
            new GroupPolicy("group3", new[] { "user3" })
        };
        var collection = new GroupCollection(groups);

        // Act
        var enumerated = collection.ToList();

        // Assert
        (enumerated.Count == 3).BeTrue();
        enumerated.All(g => collection.Contains(g)).BeTrue();
    }

    [Fact]
    public void GroupCollection_Equals_SameReference_ShouldReturnTrue()
    {
        // Arrange
        var collection = new GroupCollection(new[]
        {
            new GroupPolicy("group1", new[] { "user1" })
        });

        // Act & Assert
#pragma warning disable CS1718 // Comparison made to same variable
        (collection == collection).BeTrue();
#pragma warning restore CS1718 // Comparison made to same variable
        collection.Equals(collection).BeTrue();
    }

    [Fact]
    public void GroupCollection_Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var collection = new GroupCollection();

        // Act & Assert
        (collection == null).BeFalse();
        collection!.Equals(null).BeFalse();
    }

    [Fact]
    public void GroupCollection_Equals_DifferentOrder_ShouldBeEqual()
    {
        // Arrange
        var collection1 = new GroupCollection(new[]
        {
            new GroupPolicy("group1", new[] { "user1" }),
            new GroupPolicy("group2", new[] { "user2" })
        });

        var collection2 = new GroupCollection(new[]
        {
            new GroupPolicy("group2", new[] { "user2" }),
            new GroupPolicy("group1", new[] { "user1" })
        });

        // Act & Assert
        (collection1 == collection2).BeTrue();
    }

    [Fact]
    public void GroupCollection_Equals_DifferentCounts_ShouldNotBeEqual()
    {
        // Arrange
        var collection1 = new GroupCollection(new[]
        {
            new GroupPolicy("group1", new[] { "user1" })
        });

        var collection2 = new GroupCollection(new[]
        {
            new GroupPolicy("group1", new[] { "user1" }),
            new GroupPolicy("group2", new[] { "user2" })
        });

        // Act & Assert
        (collection1 == collection2).BeFalse();
    }

    [Fact]
    public void GroupCollection_IsReadOnly_ShouldBeFalse()
    {
        // Arrange
        var collection = new GroupCollection();

        // Act & Assert
        collection.IsReadOnly.BeFalse();
    }
}
