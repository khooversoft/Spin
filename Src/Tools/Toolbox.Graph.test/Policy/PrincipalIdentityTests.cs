using Toolbox.Tools;
using Toolbox.Extensions;

namespace Toolbox.Graph.test.Policy;

public class PrincipalIdentityTests
{
    [Fact]
    public void PolicyUserGroup_Serialization_RoundTrip()
    {
        // Arrange
        var user = new PrincipalIdentity("nameIdentfier", "user1", "email@domain.com", false);

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
            new PrincipalIdentity("nameIdentfier", "user1", "email@domain.com", false),
            new PrincipalIdentity("nameIdentfier2", "user1", "email2@domain.com", false),
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
        var v1 = new PrincipalIdentity("id1", "nameIdentfier", "user1", "email@domain.com", false);
        var v2 = new PrincipalIdentity("id1", "nameIdentfier", "user1", "email@domain.com", false);
        (v1 == v2).BeTrue();
        (v1 != v2).BeFalse();

        var v3 = new PrincipalIdentity("id2", "nameIdentfier", "user1", "email@domain.com", false);
        (v1 == v3).BeFalse();
        (v1 != v3).BeTrue();

        var v4 = new PrincipalIdentity("id1", "nameIdentfier2", "user1", "email@domain.com", false);
        (v1 == v4).BeFalse();

        var v5 = new PrincipalIdentity("id1", "nameIdentfier", "user1-x", "email@domain.com", false);
        (v1 == v4).BeFalse();

        var v6 = new PrincipalIdentity("id1", "nameIdentfier", "user1", "email@domain.com-x", false);
        (v1 == v6).BeFalse();

        var v7 = new PrincipalIdentity("id1x", "nameIdentfier", "user1", "email@domain.co4", false);
        (v1 == v7).BeFalse();
    }

    [Fact]
    public void PolicyUserGroupCollectionEdit()
    {
        var list = new PrincipalCollection();
        list.Add(new PrincipalIdentity("nameIdentfier", "user1", "email@domain.com", false));
        list.Count.Be(1);
        
        list.Add(new PrincipalIdentity("nameIdentfier2", "user1", "email2@domain.com", false));
        list.Count.Be(2);

        var list2 = new PrincipalCollection()
        {
            new PrincipalIdentity("nameIdentfier", "user1", "email@domain.com", false),
            new PrincipalIdentity("nameIdentfier2", "user1", "email2@domain.com", false),
        };

        (list == list2).BeTrue();
    }
}
