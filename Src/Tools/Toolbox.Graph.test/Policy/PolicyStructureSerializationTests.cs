using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph.test.Policy;

public class PolicyStructureSerializationTests
{
    [Fact]
    public void GrantPolicy_Serialization_RoundTrip()
    {
        // Arrange
        var originalPolicy = new GrantPolicy("customerNumber", RolePolicy.Owner | RolePolicy.NameIdentifier, "user1orGroupName");

        // Act
        var json = originalPolicy.ToJson();
        var deserializedPolicy = json.ToObject<GrantPolicy>();

        // Assert
        (deserializedPolicy != default).BeTrue();
        (originalPolicy == deserializedPolicy).BeTrue();
    }

    [Fact]
    public void GrantPolicyCollection_Serialization_RoundTrip()
    {
        // Arrange
        var originalCollection = new GrantCollection(new[]
        {
            new GrantPolicy("customerNumber", RolePolicy.Reader | RolePolicy.NameIdentifier, "user1"),
            new GrantPolicy("orderNumber", RolePolicy.Contributor | RolePolicy.SecurityGroup, "group1"),
        });

        // Act
        var json = originalCollection.ToJson();
        GrantCollection deserializedCollection = json.ToObject<GrantCollection>().NotNull();

        // Assert
        deserializedCollection.NotNull();
        deserializedCollection.Policies.Count.Be(2);
        (originalCollection == deserializedCollection).BeTrue();
        (originalCollection.Equals(deserializedCollection)).BeTrue();
    }

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
    public void GrantControl_Serialization_RoundTrip()
    {
        // Arrange
        var originalPolicy = new GrantControl(new[]
        {
            new GrantPolicy("customerNumber", RolePolicy.Owner | RolePolicy.NameIdentifier, "user1orGroupName"),
            new GrantPolicy("customerNumber2", RolePolicy.Reader | RolePolicy.SecurityGroup, "user1orGroupName2"),
        },
        new[]
        {
            new GroupPolicy("group1", new[] { "user1", "user2" }),
            new GroupPolicy("group2", new[] { "user3", "user4" }),
        });

        // Act
        var json = originalPolicy.ToJson();
        var deserializedPolicy = json.ToObject<GrantControl>();

        // Assert
        (deserializedPolicy != default).BeTrue();
        (originalPolicy == deserializedPolicy).BeTrue();
    }
}
