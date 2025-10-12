using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Policy;

public class PrincipalIdentityTests
{
    [Fact]
    public void PolicyUserGroup_Serialization_RoundTrip()
    {
        // Arrange
        var user = new PrincipalIdentity("nameIdentifier", "user1", "email@domain.com", false);

        // Act
        var json = user.ToJson();
        var deserializedUser = json.ToObject<PrincipalIdentity>();

        // Assert
        (deserializedUser != default).BeTrue();
        (user == deserializedUser).BeTrue();
    }

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
    public void EqualNotEqual()
    {
        var v1 = new PrincipalIdentity("id1", "nameIdentifier", "user1", "email@domain.com", false);
        var v2 = new PrincipalIdentity("id1", "nameIdentifier", "user1", "email@domain.com", false);
        (v1 == v2).BeTrue();
        (v1 != v2).BeFalse();

        var v3 = new PrincipalIdentity("id2", "nameIdentifier", "user1", "email@domain.com", false);
        (v1 == v3).BeFalse();
        (v1 != v3).BeTrue();

        var v4 = new PrincipalIdentity("id1", "nameIdentifier2", "user1", "email@domain.com", false);
        (v1 == v4).BeFalse();

        var v5 = new PrincipalIdentity("id1", "nameIdentifier", "user1-x", "email@domain.com", false);
        (v1 == v4).BeFalse();

        var v6 = new PrincipalIdentity("id1", "nameIdentifier", "user1", "email@domain.com-x", false);
        (v1 == v6).BeFalse();

        var v7 = new PrincipalIdentity("id1x", "nameIdentifier", "user1", "email@domain.co4", false);
        (v1 == v7).BeFalse();
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
    public void AutoPrincipalId_Generated_WithUserPrefix_And_Validates()
    {
        var user = new PrincipalIdentity("nameIdentifier", "user1", "email@domain.com", true);

        user.PrincipalId.IsNotEmpty().BeTrue();
        user.PrincipalId.StartsWith("user:").BeTrue();

        user.Validate().BeOk();
    }

    [Fact]
    public void RandomCtor_GeneratesDifferentPrincipalIds()
    {
        var a = new PrincipalIdentity("nameIdentifier", "user1", "email@domain.com");
        var b = new PrincipalIdentity("nameIdentifier", "user1", "email@domain.com");

        (a.PrincipalId != b.PrincipalId).BeTrue();
        (a == b).BeFalse();
    }

    [Fact]
    public void Validator_Fails_For_InvalidEmail()
    {
        var user = new PrincipalIdentity("nameIdentifier", "user1", "not-an-email");

        var result = user.Validate();
        result.IsError().BeTrue();
    }

    [Fact]
    public void Constructors_Throw_On_Empty_Arguments()
    {
        // First ctor (auto principalId): empty nameIdentifier
        Assert.Throws<ArgumentNullException>(() => new PrincipalIdentity("", "user1", "email@domain.com"));

        // Json ctor: empty principalId or fields
        Assert.Throws<ArgumentNullException>(() => new PrincipalIdentity("", "nameIdentifier", "user1", "email@domain.com", false));
        Assert.Throws<ArgumentNullException>(() => new PrincipalIdentity("id1", "", "user1", "email@domain.com", false));
        Assert.Throws<ArgumentNullException>(() => new PrincipalIdentity("id1", "nameIdentifier", "", "email@domain.com", false));
        Assert.Throws<ArgumentNullException>(() => new PrincipalIdentity("id1", "nameIdentifier", "user1", "", false));
    }

    [Fact]
    public void Serialization_Preserves_EmailConfirmed()
    {
        var user = new PrincipalIdentity("nameIdentifier", "user1", "email@domain.com", true);

        var json = user.ToJson();
        var roundtrip = json.ToObject<PrincipalIdentity>().NotNull();

        (roundtrip == user).BeTrue();
        roundtrip.EmailConfirmed.BeTrue();
    }
}
