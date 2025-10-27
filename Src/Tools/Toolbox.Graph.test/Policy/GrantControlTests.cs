using Toolbox.Tools;

namespace Toolbox.Graph.test.Policy;

public class GrantControlTests
{
    [Fact]
    public void GrantControl_Default_Constructor_Has_Empty_Collections()
    {
        var subject = new GrantControl();

        subject.Principals.Count.Be(0);
        subject.Groups.Count.Be(0);
    }

    [Fact]
    public void GrantControl_Constructor_Populates_Collections()
    {
        var groups = new[]
        {
            new GroupPolicy("g1", new[] { "u1", "u2" }),
            new GroupPolicy("g2", new[] { "u3" }),
        };

        var principals = new[]
        {
            new PrincipalIdentity("u1", "nid1", "user1", "user1@domain.com", false),
            new PrincipalIdentity("u2", "nid2", "user2", "user2@domain.com", true),
        };

        var subject = new GrantControl(groups, principals);

        subject.Principals.Count.Be(2);
        subject.Groups.Count.Be(2);
    }

    [Fact]
    public void GrantControl_Equality_SameContent_DifferentOrder()
    {
        var groupsA = new[]
        {
            new GroupPolicy("g1", new[] { "u1", "u2" }),
            new GroupPolicy("g2", new[] { "u3" }),
        };

        var principalsA = new[]
        {
            new PrincipalIdentity("u1", "nid1", "user1", "user1@domain.com", false),
            new PrincipalIdentity("u2", "nid2", "user2", "user2@domain.com", true),
        };

        var groupsB = new[]
        {
            new GroupPolicy("g2", new[] { "u3" }),
            new GroupPolicy("g1", new[] { "u1", "u2" }),
        };

        var principalsB = new[]
        {
            new PrincipalIdentity("u2", "nid2", "user2", "user2@domain.com", true),
            new PrincipalIdentity("u1", "nid1", "user1", "user1@domain.com", false),
        };

        var a = new GrantControl(groupsA, principalsA);
        var b = new GrantControl(groupsB, principalsB);

        (a == b).BeTrue();
        a.Equals(b).BeTrue();
        (a != b).BeFalse();
    }

    [Fact]
    public void GrantControl_Operator_Null_Semantics_Documented()
    {
        var subject = new GrantControl();

        // Proper null semantics: non-null != null
        (subject == null).BeFalse();
        (subject != null).BeTrue();

        GrantControl? n1 = null;
        GrantControl? n2 = null;
        (n1 == n2).BeTrue();    // null == null should be true
        (n1 != n2).BeFalse();   // null != null should be false
    }

    [Fact]
    public void GrantControl_HasAccess_Smoke_Tests_Direct_Group_Unauthorized_And_Default()
    {
        var groups = new[]
        {
            new GroupPolicy("groupR", new[] { "u2", "u3" }),
        };

        var principals = new[]
        {
            // Use explicit principalId so AccessRequest principalIdentifier matches
            new PrincipalIdentity("u1", "nid1", "user1", "user1@domain.com", false),
            new PrincipalIdentity("u2", "nid2", "user2", "user2@domain.com", true),
            // Note: u3 is NOT present in principals to verify pre-check failure
        };

        var grants = new[]
        {
            // Direct contributor for u1 on res1
            new GrantPolicy("res1", RolePolicy.Contributor | RolePolicy.PrincipalIdentity, "u1"),
            // Group reader for groupR on res2
            new GrantPolicy("res2", RolePolicy.Reader | RolePolicy.SecurityGroup, "groupR"),
        };

        var ctrl = new GrantControl(groups, principals);

        // Direct access: contributor => Create allowed
        ctrl.HasAccess(new AccessRequest(AccessType.Create, "u1", "res1"), grants).BeTrue();
        // Reader should not allow AssignRole
        ctrl.HasAccess(new AccessRequest(AccessType.AssignRole, "u2", "res2"), grants).BeFalse();

        // Group-based access for u2 (member of groupR): Get allowed
        ctrl.HasAccess(new AccessRequest(AccessType.Get, "u2", "res2"), grants).BeTrue();

        // Principal not known to ctrl.Principals even if group membership exists => deny
        ctrl.HasAccess(new AccessRequest(AccessType.Get, "u3", "res2"), grants).BeFalse();

        // Unprotected resource (no policy for resX) => default allow
        ctrl.HasAccess(new AccessRequest(AccessType.Get, "u1", "resX"), grants).BeTrue();
    }

    [Fact]
    public void HasAccess_EmptyGrantCollection_Returns_True_For_Unprotected_Resources()
    {
        var principals = new[] { new PrincipalIdentity("u1", "nid1", "user1", "user1@domain.com", false) };
        var ctrl = new GrantControl([], principals);

        ctrl.HasAccess(new AccessRequest(AccessType.Get, "u1", "anyResource"), []).BeTrue();
    }

    [Fact]
    public void HasAccess_PrincipalNotInCollection_Returns_False()
    {
        var ctrl = new GrantControl([], []);

        ctrl.HasAccess(new AccessRequest(AccessType.Get, "unknownUser", "res1"), []).BeFalse();
    }

    [Fact]
    public void HasAccess_MultipleGroupMemberships_GrantsAccess_If_Any_Match()
    {
        var groups = new[]
        {
            new GroupPolicy("groupA", new[] { "u1" }),
            new GroupPolicy("groupB", new[] { "u1" }),
        };
        var principals = new[] { new PrincipalIdentity("u1", "nid1", "user1", "user1@domain.com", false) };

        var grants = new[]
        {
            new GrantPolicy("res1", RolePolicy.Reader | RolePolicy.SecurityGroup, "groupB")
        };

        var ctrl = new GrantControl(groups, principals);
        ctrl.HasAccess(new AccessRequest(AccessType.Get, "u1", "res1"), grants).BeTrue();
    }

    [Fact]
    public void HasAccess_AllAccessTypes_With_Owner_Role()
    {
        var principals = new[] { new PrincipalIdentity("u1", "nid1", "user1", "user1@domain.com", false) };
        var grants = new[]
        {
            new GrantPolicy("res1", RolePolicy.Owner | RolePolicy.PrincipalIdentity, "u1")
        };

        var ctrl = new GrantControl([], principals);

        ctrl.HasAccess(new AccessRequest(AccessType.Get, "u1", "res1"), grants).BeTrue();
        ctrl.HasAccess(new AccessRequest(AccessType.Create, "u1", "res1"), grants).BeTrue();
        ctrl.HasAccess(new AccessRequest(AccessType.Update, "u1", "res1"), grants).BeTrue();
        ctrl.HasAccess(new AccessRequest(AccessType.Delete, "u1", "res1"), grants).BeTrue();
        ctrl.HasAccess(new AccessRequest(AccessType.AssignRole, "u1", "res1"), grants).BeTrue();
    }

    [Fact]
    public void Equality_EmptyCollections_Are_Equal()
    {
        var a = new GrantControl();
        var b = new GrantControl();

        (a == b).BeTrue();
        a.GetHashCode().Be(b.GetHashCode());
    }

    [Fact]
    public void GrantControl_HashCode_Consistent_With_Equality()
    {
        var groups = new[] { new GroupPolicy("g1", new[] { "u1" }) };
        var principals = new[] { new PrincipalIdentity("u1", "nid1", "user1", "user1@domain.com", false) };

        var a = new GrantControl(groups, principals);
        var b = new GrantControl(groups, principals);

        (a == b).BeTrue();
        a.GetHashCode().Be(b.GetHashCode());
    }
}
