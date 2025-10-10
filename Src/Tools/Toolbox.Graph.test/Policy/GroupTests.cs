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
}
