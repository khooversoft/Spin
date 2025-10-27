using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph.test.Policy;

public class GrantPolicyTests
{
    [Fact]
    public void GrantPolicy_Serialization_RoundTrip()
    {
        // Arrange
        var originalPolicy = new GrantPolicy("customerNumber", RolePolicy.Owner | RolePolicy.PrincipalIdentity, "user1orGroupName");

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
        var originalPolicy = new GrantPolicy("customerNumber", RolePolicy.Owner | RolePolicy.PrincipalIdentity, "user1orGroupName");

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
        var originalCollection = new[]
        {
            new GrantPolicy("customerNumber", RolePolicy.Reader | RolePolicy.PrincipalIdentity, "user1"),
            new GrantPolicy("orderNumber", RolePolicy.Contributor | RolePolicy.SecurityGroup, "group1"),
        };

        // Act
        var json = originalCollection.ToJson();
        var deserializedCollection = json.ToObject<IReadOnlyList<GrantPolicy>>().NotNull();

        // Assert
        deserializedCollection.NotNull();
        deserializedCollection.Count.Be(2);
        originalCollection.SequenceEqual(deserializedCollection).BeTrue();
    }

    // ---- GrantPolicy specific tests ----

    [Fact]
    public void GrantPolicy_Encode_Parse_RoundTrip()
    {
        var original = new GrantPolicy("name1", RolePolicy.Owner | RolePolicy.PrincipalIdentity, "user1");
        var encoded = original.Encode();

        // Format should be: name:o:ni:principal
        encoded.Contains(":").BeTrue();
        encoded.StartsWith("name1:").BeTrue();

        var parsed = GrantPolicy.Parse(encoded);

        (parsed == original).BeTrue();
        parsed.NameIdentifier.Be("name1");
        parsed.PrincipalIdentifier.Be("user1");
        parsed.RoleNumeric.Be((int)(RolePolicy.Owner | RolePolicy.PrincipalIdentity));
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
        var policy = new GrantPolicy("n", RolePolicy.Reader | RolePolicy.PrincipalIdentity, "u");
        policy.RoleNumeric.Be((int)(RolePolicy.Reader | RolePolicy.PrincipalIdentity));
    }

    [Fact]
    public void GrantPolicy_Ctor_Throws_On_Invalid_Args()
    {
        Assert.Throws<ArgumentNullException>(() => new GrantPolicy("", RolePolicy.Reader | RolePolicy.PrincipalIdentity, "u"));
        Assert.Throws<ArgumentException>(() => new GrantPolicy("n", RolePolicy.None, "u"));
        Assert.Throws<ArgumentNullException>(() => new GrantPolicy("n", RolePolicy.Reader | RolePolicy.PrincipalIdentity, null!));
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
        var a1 = new GrantPolicy("n1", RolePolicy.Reader | RolePolicy.PrincipalIdentity, "u1");
        var a2 = new GrantPolicy("n1", RolePolicy.Reader | RolePolicy.PrincipalIdentity, "u1");
        var b = new GrantPolicy("n1", RolePolicy.Contributor | RolePolicy.PrincipalIdentity, "u1");
        var c = new GrantPolicy("n1", RolePolicy.Reader | RolePolicy.PrincipalIdentity, "u2");

        (a1 == a2).BeTrue();
        (a1 != a2).BeFalse();
        a1.Equals(a2).BeTrue();
        (a1.GetHashCode() == a2.GetHashCode()).BeTrue();

        (a1 == b).BeFalse();
        (a1 == c).BeFalse();
    }

    [Fact]
    public void GrantPolicy_JsonConstructor_WithRoleNumeric()
    {
        // Arrange & Act
        var policy = new GrantPolicy("name1", 17, "user1"); // 17 = Reader | PrincipalIdentity

        // Assert
        policy.NameIdentifier.Be("name1");
        policy.Role.Be(RolePolicy.Reader | RolePolicy.PrincipalIdentity);
        policy.RoleNumeric.Be(17);
        policy.PrincipalIdentifier.Be("user1");
    }

    [Fact]
    public void GrantPolicy_JsonConstructor_InvalidRole_Throws()
    {
        // Role.None = 0 is invalid
        Assert.Throws<ArgumentException>(() => new GrantPolicy("name", 0, "user"));
    }

    [Fact]
    public void GrantPolicy_Validator_ValidatesCorrectly()
    {
        var validPolicy = new GrantPolicy("name", RolePolicy.Reader | RolePolicy.PrincipalIdentity, "user");
        validPolicy.Validate().BeOk();
    }

    [Fact]
    public void GrantPolicy_Properties_ReturnsCorrectValues()
    {
        var policy = new GrantPolicy("testNode", RolePolicy.Contributor | RolePolicy.SecurityGroup, "group1");

        policy.NameIdentifier.Be("testNode");
        policy.Role.Be(RolePolicy.Contributor | RolePolicy.SecurityGroup);
        policy.PrincipalIdentifier.Be("group1");
    }

    [Fact]
    public void GrantPolicy_Parse_AllRoleCombinations()
    {
        // Test all valid role combinations
        var testCases = new[]
        {
            ("name:r:pi:user", RolePolicy.Reader | RolePolicy.PrincipalIdentity),
            ("name:c:pi:user", RolePolicy.Contributor | RolePolicy.PrincipalIdentity),
            ("name:o:pi:user", RolePolicy.Owner | RolePolicy.PrincipalIdentity),
            ("name:r:sg:group", RolePolicy.Reader | RolePolicy.SecurityGroup),
            ("name:c:sg:group", RolePolicy.Contributor | RolePolicy.SecurityGroup),
            ("name:o:sg:group", RolePolicy.Owner | RolePolicy.SecurityGroup),
        };

        foreach (var (encoded, expectedRole) in testCases)
        {
            var parsed = GrantPolicy.Parse(encoded);
            parsed.Role.Be(expectedRole);
        }
    }

    [Fact]
    public void GrantPolicy_Parse_SpecialCharactersInIdentifiers()
    {
        // Test with special characters that might be problematic
        var policy = new GrantPolicy("node-123_test", RolePolicy.Reader | RolePolicy.PrincipalIdentity, "user@domain.com");
        var encoded = policy.Encode();
        var parsed = GrantPolicy.Parse(encoded);

        (parsed == policy).BeTrue();
    }

    [Fact]
    public void GrantPolicy_Parse_TooManyColons()
    {
        Assert.Throws<ArgumentException>(() => GrantPolicy.Parse("name:o:pi:user:extra"));
    }

    [Fact]
    public void GrantPolicy_Parse_EmptyString()
    {
        Assert.Throws<ArgumentException>(() => GrantPolicy.Parse(""));
    }

    [Fact]
    public void GrantPolicy_Parse_NullString()
    {
        Assert.Throws<ArgumentException>(() => GrantPolicy.Parse(null!));
    }
}
