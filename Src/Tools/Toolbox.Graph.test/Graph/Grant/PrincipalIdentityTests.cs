using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Graph.Grant;

public class PrincipalIdentityTests
{
    [Fact]
    public void PrincipalIdentityCreate()
    {
        var subject = new PrincipalIdentity("user1", "userName1", "email1@domain.com", true);
        subject.PrincipalId.Be("user1");
        subject.NameIdentifier.Be("user1");
        subject.UserName.Be("userName1");
        subject.Email.Be("email1@domain.com");
        subject.EmailConfirmed.BeTrue();
        subject.NodeKey.Be("principal:user1");
        subject.Validate().ThrowOnError();

        var subject2 = new PrincipalIdentity("user1", "userName1", "email1@domain.com", true);
        (subject == subject2).BeTrue();

        var json = subject.ToJson();
        var read = json.ToObject<PrincipalIdentity>();
        read.Be(subject);
        (read == subject).BeTrue();
    }

    [Fact]
    public void PrincipalIdentityCreate_DefaultEmailConfirmed()
    {
        var subject = new PrincipalIdentity("user1", "userName1", "email1@domain.com");
        subject.PrincipalId.Be("user1");
        subject.NameIdentifier.Be("user1");
        subject.UserName.Be("userName1");
        subject.Email.Be("email1@domain.com");
        subject.EmailConfirmed.BeFalse();
        subject.NodeKey.Be("principal:user1");
        subject.Validate().ThrowOnError();
    }

    [Fact]
    public void PrincipalIdentityCreate_JsonConstructor()
    {
        var subject = new PrincipalIdentity("user1", "nameId123", "userName1", "email1@domain.com", true);
        subject.PrincipalId.Be("user1");
        subject.NameIdentifier.Be("nameId123");
        subject.UserName.Be("userName1");
        subject.Email.Be("email1@domain.com");
        subject.EmailConfirmed.BeTrue();
        subject.NodeKey.Be("principal:user1");
        subject.Validate().ThrowOnError();
    }

    [Fact]
    public void PrincipalIdentityCreate_EmptyPrincipalId_ThrowsException()
    {
        Verify.Throws<ArgumentException>(() => new PrincipalIdentity("", "userName1", "email1@domain.com", true));
    }

    [Fact]
    public void PrincipalIdentityCreate_EmptyUserName_ThrowsException()
    {
        Verify.Throws<ArgumentException>(() => new PrincipalIdentity("user1", "", "email1@domain.com", true));
    }

    [Fact]
    public void PrincipalIdentityCreate_EmptyEmail_ThrowsException()
    {
        Verify.Throws<ArgumentException>(() => new PrincipalIdentity("user1", "userName1", "", true));
    }

    [Fact]
    public void PrincipalIdentityCreate_EmptyNameIdentifier_ThrowsException()
    {
        Verify.Throws<ArgumentException>(() => new PrincipalIdentity("user1", "", "userName1", "email1@domain.com", true));
    }

    [Fact]
    public void PrincipalIdentityValidation_InvalidEmail_Fails()
    {
        var subject = new PrincipalIdentity("user1", "user1", "userName1", "invalid-email", false);
        var result = subject.Validate();
        result.IsError().BeTrue();
    }

    [Fact]
    public void PrincipalIdentityUpdate_AllFields()
    {
        var subject = new PrincipalIdentity("user1", "userName1", "email1@domain.com", false);
        var updated = subject.Update("newNameId", "newUserName", "newemail@domain.com", true);

        updated.PrincipalId.Be("user1");
        updated.NameIdentifier.Be("newNameId");
        updated.UserName.Be("newUserName");
        updated.Email.Be("newemail@domain.com");
        updated.EmailConfirmed.BeTrue();
        updated.NodeKey.Be("principal:user1");
    }

    [Fact]
    public void PrincipalIdentityUpdate_PartialFields()
    {
        var subject = new PrincipalIdentity("user1", "userName1", "email1@domain.com", false);
        var updated = subject.Update(null, "newUserName", null, null);

        updated.PrincipalId.Be("user1");
        updated.NameIdentifier.Be("user1");
        updated.UserName.Be("newUserName");
        updated.Email.Be("email1@domain.com");
        updated.EmailConfirmed.BeFalse();
    }

    [Fact]
    public void PrincipalIdentityUpdate_NoFields()
    {
        var subject = new PrincipalIdentity("user1", "userName1", "email1@domain.com", true);
        var updated = subject.Update(null, null, null, null);

        updated.PrincipalId.Be("user1");
        updated.NameIdentifier.Be("user1");
        updated.UserName.Be("userName1");
        updated.Email.Be("email1@domain.com");
        updated.EmailConfirmed.BeTrue();
    }

    [Fact]
    public void PrincipalIdentityUpdate_EmptyStringsIgnored()
    {
        var subject = new PrincipalIdentity("user1", "userName1", "email1@domain.com", false);
        var updated = subject.Update("", "", "", null);

        updated.PrincipalId.Be("user1");
        updated.NameIdentifier.Be("user1");
        updated.UserName.Be("userName1");
        updated.Email.Be("email1@domain.com");
        updated.EmailConfirmed.BeFalse();
    }

    [Fact]
    public void PrincipalIdentityCreateNameIdentifierNodeKey()
    {
        var subject = new PrincipalIdentity("user1", "nameId123", "userName1", "email1@domain.com", true);
        var nodeKey = subject.CreateNameIdentifierNodeKey();
        nodeKey.Be("nameidentifier:nameid123");
    }

    [Fact]
    public void PrincipalIdentityCreateUserNameNodeKey()
    {
        var subject = new PrincipalIdentity("user1", "userName1", "email1@domain.com", true);
        var nodeKey = subject.CreateUserNameNodeKey();
        nodeKey.Be("username:username1");
    }

    [Fact]
    public void PrincipalIdentityCreateEmailNodeKey()
    {
        var subject = new PrincipalIdentity("user1", "userName1", "email1@domain.com", true);
        var nodeKey = subject.CreateEmailNodeKey();
        nodeKey.Be("email:email1@domain.com");
    }

    [Fact]
    public void PrincipalIdentityIsNodeType_ValidNodeKey_ReturnsTrue()
    {
        var nodeKey = "principal:principal";
        var result = PrincipalIdentityTool.IsNodeType(nodeKey);
        result.BeTrue();
    }

    [Fact]
    public void PrincipalIdentityIsNodeType_InvalidNodeKey_ReturnsFalse()
    {
        var nodeKey = "principal:other-type";
        var result = PrincipalIdentityTool.IsNodeType(nodeKey);
        result.BeTrue();
    }

    [Fact]
    public void PrincipalIdentityRecordEquality()
    {
        var subject1 = new PrincipalIdentity("user1", "userName1", "email1@domain.com", true);
        var subject2 = new PrincipalIdentity("user1", "userName1", "email1@domain.com", true);
        var subject3 = new PrincipalIdentity("user2", "userName1", "email1@domain.com", true);

        (subject1 == subject2).BeTrue();
        (subject1 != subject3).BeTrue();
        subject1.Equals(subject2).BeTrue();
        subject1.Equals(subject3).BeFalse();
    }

    [Fact]
    public void PrincipalIdentityWithExpression()
    {
        var subject = new PrincipalIdentity("user1", "userName1", "email1@domain.com", false);
        var modified = subject with { EmailConfirmed = true };

        subject.EmailConfirmed.BeFalse();
        modified.EmailConfirmed.BeTrue();
        modified.PrincipalId.Be("user1");
        modified.UserName.Be("userName1");
        modified.Email.Be("email1@domain.com");
    }

    [Fact]
    public void PrincipalIdentityConstants()
    {
        PrincipalIdentity.NodeType.Be("principal");
        PrincipalIdentity.NodeReferenceType.Be("principal-ref");
        PrincipalIdentity.NameIdentifierClaimType.Be("nameidentifier");
        PrincipalIdentity.UserNameClaimType.Be("username");
        PrincipalIdentity.EmailClaimType.Be("email");
    }

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
        (v1 == v5).BeFalse(); // fixed: compare v1 with v5 (not v4)

        var v6 = new PrincipalIdentity("id1", "nameIdentifier", "user1", "email@domain.com-x", false);
        (v1 == v6).BeFalse();

        var v7 = new PrincipalIdentity("id1x", "nameIdentifier", "user1", "email@domain.co4", false);
        (v1 == v7).BeFalse();
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

    [Fact]
    public void Default_EmailConfirmed_Is_False()
    {
        var user = new PrincipalIdentity("nameIdentifier", "user1", "email@domain.com");
        user.EmailConfirmed.BeFalse();
    }

    [Fact]
    public void Validator_Ok_For_ExplicitCtor()
    {
        var user = new PrincipalIdentity("id1", "nameIdentifier", "user1", "email@domain.com", false);
        user.Validate().BeOk();
    }
}
