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
    public void Serialization_roundtrip()
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
        deserializedCollection.Count.Be(2);
        (originalCollection == deserializedCollection).BeTrue();
        (originalCollection.Equals(deserializedCollection)).BeTrue();
    }

    // ---- GrantPolicy specific tests ----

    [Fact]
    public void GrantPolicy_Encode_Parse_RoundTrip()
    {
        var original = new GrantPolicy("name1", RolePolicy.Owner | RolePolicy.NameIdentifier, "user1");
        var encoded = original.Encode();

        // Format should be: name:o:ni:principal
        encoded.Contains(":").BeTrue();
        encoded.StartsWith("name1:").BeTrue();

        var parsed = GrantPolicy.Parse(encoded);

        (parsed == original).BeTrue();
        parsed.NameIdentifier.Be("name1");
        parsed.PrincipalIdentifier.Be("user1");
        parsed.RoleNumeric.Be((int)(RolePolicy.Owner | RolePolicy.NameIdentifier));
    }

    [Fact]
    public void GrantPolicy_ToString_And_Implicit_String_Return_Encode()
    {
        var policy = new GrantPolicy("nodeX", RolePolicy.Contributor | RolePolicy.SecurityGroup, "groupA");
        var encoded = policy.Encode();

        policy.ToString().Be(encoded);

        string s = policy; // implicit operator
        s.Be(encoded);
    }

    [Fact]
    public void GrantPolicy_RoleNumeric_Matches_Role()
    {
        var policy = new GrantPolicy("n", RolePolicy.Reader | RolePolicy.NameIdentifier, "u");
        policy.RoleNumeric.Be((int)(RolePolicy.Reader | RolePolicy.NameIdentifier));
    }

    [Fact]
    public void GrantPolicy_Ctor_Throws_On_Invalid_Args()
    {
        Assert.Throws<ArgumentNullException>(() => new GrantPolicy("", RolePolicy.Reader | RolePolicy.NameIdentifier, "u"));
        Assert.Throws<ArgumentException>(() => new GrantPolicy("n", RolePolicy.None, "u"));
        Assert.Throws<ArgumentNullException>(() => new GrantPolicy("n", RolePolicy.Reader | RolePolicy.NameIdentifier, null!));
    }

    [Fact]
    public void GrantPolicy_Parse_Throws_On_Invalid_Formats()
    {
        // Not enough parts
        Assert.Throws<ArgumentException>(() => GrantPolicy.Parse("a:b:c"));

        // Empty name identifier
        Assert.Throws<ArgumentException>(() => GrantPolicy.Parse(":o:ni:user"));

        // Empty principal
        Assert.Throws<ArgumentException>(() => GrantPolicy.Parse("name:o:ni:"));

        // Invalid schema/role tokens
        Assert.Throws<ArgumentException>(() => GrantPolicy.Parse("name:x:ni:user")); // invalid role token 'x'
        Assert.Throws<ArgumentException>(() => GrantPolicy.Parse("name:o:xx:user")); // invalid schema token 'xx'
    }

    [Fact]
    public void GrantPolicy_Equality_And_HashCode()
    {
        var a1 = new GrantPolicy("n1", RolePolicy.Reader | RolePolicy.NameIdentifier, "u1");
        var a2 = new GrantPolicy("n1", RolePolicy.Reader | RolePolicy.NameIdentifier, "u1");
        var b = new GrantPolicy("n1", RolePolicy.Contributor | RolePolicy.NameIdentifier, "u1");
        var c = new GrantPolicy("n1", RolePolicy.Reader | RolePolicy.NameIdentifier, "u2");

        (a1 == a2).BeTrue();
        (a1 != a2).BeFalse();
        a1.Equals(a2).BeTrue();
        (a1.GetHashCode() == a2.GetHashCode()).BeTrue();

        (a1 == b).BeFalse();
        (a1 == c).BeFalse();
    }
}
