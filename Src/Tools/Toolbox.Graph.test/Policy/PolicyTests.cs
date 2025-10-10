using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph.test.Policy;

public class PolicyTests
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
    public void Searlization_roundtrip()
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
}
