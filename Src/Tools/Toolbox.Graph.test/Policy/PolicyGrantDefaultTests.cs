using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph.test.Policy;

public class PolicyGrantDefaultTests
{
    [Fact]
    public void NoPolicy_DefaultAccess()
    {
        var grantPolicies = new GrantCollection()
        {
        };

        var groupPolicies = new[]
        {
            new GroupPolicy("group1", new[] { "user1", "user2" }),
            new GroupPolicy("group2", new[] { "user3", "user4" }),
        };

        var principals = new[]
        {
            new PrincipalIdentity("user:id1", "id1", "user1", "user1@domain.com", false),
            new PrincipalIdentity("user:id2", "id2", "user2", "user2@domain.com", true),
        };

        var grantControl = new GrantControl(groupPolicies, principals);

        // Not protected, should have access
        var request = new AccessRequest(AccessType.Get, "user:id1", "customer3");

        grantControl.HasAccess(request, grantPolicies).BeTrue();
    }

    [Fact]
    public void GrantPolicy_Defaults()
    {
        var grantPolicies = new GrantCollection()
        {
            new GrantPolicy("customer1", RolePolicy.Owner | RolePolicy.PrincipalIdentity, "user1orGroupName"),
            new GrantPolicy("customer2", RolePolicy.Reader | RolePolicy.SecurityGroup, "user1orGroupName2"),
        };

        var groupPolicies = new[]
        {
            new GroupPolicy("group1", new[] { "user1", "user2" }),
            new GroupPolicy("group2", new[] { "user3", "user4" }),
        };

        var principals = new[]
        {
            new PrincipalIdentity("user:id1", "id1", "user1", "user1@domain.com", false),
            new PrincipalIdentity("user:id2", "id2", "user2", "user2@domain.com", true),
        };

        var grantControl = new GrantControl(groupPolicies, principals);

        // Not protected, should have access
        var request = new AccessRequest(AccessType.Get, "user:id1", "customer3");

        grantControl.HasAccess(request, grantPolicies).BeTrue();
    }

    [Fact]
    public void EqualNotEqual()
    {
        var v1 = new GrantPolicy("customer1", RolePolicy.Owner | RolePolicy.PrincipalIdentity, "user1orGroupName");
        var v2 = new GrantPolicy("customer1", RolePolicy.Owner | RolePolicy.PrincipalIdentity, "user1orGroupName");
        (v1 == v2).BeTrue();
        (v1 != v2).BeFalse();

        var v3 = new GrantPolicy("customer12", RolePolicy.Owner | RolePolicy.PrincipalIdentity, "user1orGroupName");
        (v1 == v3).BeFalse();
        (v1 != v3).BeTrue();

        var v4 = new GrantPolicy("customer1", RolePolicy.Contributor | RolePolicy.PrincipalIdentity, "user1orGroupName");
        (v1 == v3).BeFalse();

        var v5 = new GrantPolicy("customer1", RolePolicy.Owner | RolePolicy.PrincipalIdentity, "user1orGroupName-bad");
        (v1 == v5).BeFalse();
    }

    [Fact]
    public void GrantPolicy_Validate_GetAccess()
    {
        var grantPolicies = new GrantCollection()
        {
            new GrantPolicy("customer1", RolePolicy.Reader | RolePolicy.PrincipalIdentity, "user1"),
            new GrantPolicy("customer1", RolePolicy.Contributor | RolePolicy.PrincipalIdentity, "user2"),
            new GrantPolicy("customer1", RolePolicy.Owner | RolePolicy.PrincipalIdentity, "user3"),
        };

        var principals = new[]
        {
            new PrincipalIdentity("user1", "id1", "user1Name", "user1@domain.com", false),
            new PrincipalIdentity("user2", "id2", "user2Name", "user2@domain.com", true),
            new PrincipalIdentity("user3", "id3", "user3Name", "user2@domain.com", true),
        };

        var grantControl = new GrantControl([], principals);

        // Protected, but no access
        new AccessRequest(AccessType.Get, "user_no", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.Create, "user_no", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.Update, "user_no", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.Delete, "user_no", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.AssignRole, "user_no", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());

        // Protected, should have access
        new AccessRequest(AccessType.Get, "user1", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
        new AccessRequest(AccessType.Create, "user1", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.Update, "user1", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.Delete, "user1", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.AssignRole, "user1", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());

        // protected, but does not should have access
        new AccessRequest(AccessType.Get, "user2", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
        new AccessRequest(AccessType.Create, "user2", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
        new AccessRequest(AccessType.Update, "user2", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
        new AccessRequest(AccessType.Delete, "user2", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
        new AccessRequest(AccessType.AssignRole, "user2", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());

        // protected, but does not should have access
        new AccessRequest(AccessType.Get, "user3", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
        new AccessRequest(AccessType.Create, "user3", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
        new AccessRequest(AccessType.Update, "user3", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
        new AccessRequest(AccessType.Delete, "user3", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
        new AccessRequest(AccessType.AssignRole, "user3", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
    }

    [Fact]
    public void GrantPolicyGroup_Validate_GetAccess()
    {
        var grantPolicies = new GrantCollection()
        {
            new GrantPolicy("customer1", RolePolicy.Reader | RolePolicy.SecurityGroup, "group1"),
            new GrantPolicy("customer1", RolePolicy.Contributor | RolePolicy.SecurityGroup, "group2"),
            new GrantPolicy("customer1", RolePolicy.Owner | RolePolicy.SecurityGroup, "group3"),
        };

        var groupPolicies = new[]
        {
            new GroupPolicy("group1", ["user1", "user9"]),
            new GroupPolicy("group2", ["user2", "user5"]),
            new GroupPolicy("group3", ["user3", "user5"]),
            new GroupPolicy("group4", ["user4", "user6"]),
        };

        var principals = new[]
{
            new PrincipalIdentity("user1", "user:id1", "user1", "user1@domain.com", false),
            new PrincipalIdentity("user2", "user:id2", "user2", "user2@domain.com", true),
            new PrincipalIdentity("user3", "user:id3", "user3", "user3@domain.com", true),
            new PrincipalIdentity("user4", "user:id4", "user4", "user4@domain.com", true),
        };

        var grantControl = new GrantControl(groupPolicies, principals);

        // Not protected, should have access
        new AccessRequest(AccessType.Get, "user4", "not-protected").Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());

        // Protected, but no access
        new AccessRequest(AccessType.Get, "user4", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.Create, "user4", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.Update, "user4", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.Delete, "user4", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.AssignRole, "user4", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());

        // Protected, but no access
        new AccessRequest(AccessType.Get, "user99", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.Create, "user99", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.Update, "user99", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.Delete, "user99", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.AssignRole, "user99", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());

        // Protected, but no access
        new AccessRequest(AccessType.Get, "user4", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.Create, "user4", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.Update, "user4", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.Delete, "user4", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.AssignRole, "user4", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());

        // Protected, should have access
        new AccessRequest(AccessType.Get, "user1", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
        new AccessRequest(AccessType.Create, "user1", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.Update, "user1", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.Delete, "user1", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
        new AccessRequest(AccessType.AssignRole, "user1", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());

        // Protected, should have access
        new AccessRequest(AccessType.Get, "user2", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
        new AccessRequest(AccessType.Create, "user2", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
        new AccessRequest(AccessType.Update, "user2", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
        new AccessRequest(AccessType.Delete, "user2", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
        new AccessRequest(AccessType.AssignRole, "user2", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());

        // protected, but does not should have access
        new AccessRequest(AccessType.Get, "user3", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
        new AccessRequest(AccessType.Create, "user3", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
        new AccessRequest(AccessType.Update, "user3", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
        new AccessRequest(AccessType.Delete, "user3", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
        new AccessRequest(AccessType.AssignRole, "user3", "customer1").Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
    }

    // Missing: User not in Principals collection
    [Fact]
    public void UnknownPrincipal_DeniedAccess()
    {
        var grantPolicies = new GrantCollection()
        {
            new GrantPolicy("customer1", RolePolicy.Reader | RolePolicy.PrincipalIdentity, "user1"),
        };
        
        var principals = new[]
        {
            new PrincipalIdentity("user1", "id1", "user1", "user1@domain.com", false),
        };
        
        var grantControl = new GrantControl([], principals);
        
        // User exists in policy butnot in principals - should be denied
        new AccessRequest(AccessType.Get, "unknown-user", "customer1")
            .Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
    }

    // Missing: User has both direct and group-based access
    [Fact]
    public void MixedAccess_DirectAndGroup()
    {
        var grantPolicies = new GrantCollection()
        {
            new GrantPolicy("customer1", RolePolicy.Reader | RolePolicy.PrincipalIdentity, "user1"),
            new GrantPolicy("customer1", RolePolicy.Owner | RolePolicy.SecurityGroup, "group1"),
        };
        
        var groupPolicies = new[]
        {
            new GroupPolicy("group1", ["user1"]),
        };
        
        var principals = new[]
        {
            new PrincipalIdentity("user1", "id1", "user1", "user1@domain.com", false),
        };
        
        var grantControl = new GrantControl(groupPolicies, principals);
        
        // Should have highest privilege (Owner via group)
        new AccessRequest(AccessType.AssignRole, "user1", "customer1")
            .Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
    }

    // Missing: User access to different resources
    [Fact]
    public void MultipleResources_DifferentAccess()
    {
        var grantPolicies = new GrantCollection()
        {
            new GrantPolicy("customer1", RolePolicy.Owner | RolePolicy.PrincipalIdentity, "user1"),
            new GrantPolicy("customer2", RolePolicy.Reader | RolePolicy.PrincipalIdentity, "user1"),
        };
        
        var principals = new[]
        {
            new PrincipalIdentity("user1", "id1", "user1", "user1@domain.com", false),
        };
        
        var grantControl = new GrantControl([], principals);
        
        // Owner on customer1
        new AccessRequest(AccessType.Delete, "user1", "customer1")
            .Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
        
        // Only Reader on customer2
        new AccessRequest(AccessType.Delete, "user1", "customer2")
            .Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
    }

    // Missing: Tests for GrantControl.Equals, GetHashCode, operators
    [Fact]
    public void GrantControl_Equality()
    {
        var principals = new[] { new PrincipalIdentity("user1", "id1", "user1", "user1@domain.com", false) };
        var groups = new[] { new GroupPolicy("group1", ["user1"]) };
        
        var gc1 = new GrantControl(groups, principals);
        var gc2 = new GrantControl(groups, principals);
        
        (gc1 == gc2).BeTrue();
        gc1.Equals(gc2).BeTrue();
        gc1.GetHashCode().Be(gc2.GetHashCode());
    }

    // Missing: Empty principals, empty groups, both empty
    [Fact]
    public void EmptyPrincipals_DeniedAccess()
    {
        var grantPolicies = new GrantCollection()
        {
            new GrantPolicy("customer1", RolePolicy.Reader | RolePolicy.PrincipalIdentity, "user1"),
        };
        
        var grantControl = new GrantControl([], []);
        
        new AccessRequest(AccessType.Get, "user1", "customer1")
            .Action(x => grantControl.HasAccess(x, grantPolicies).BeFalse());
    }

    // Missing: User belongs to multiple groups with different access levels
    [Fact]
    public void UserInMultipleGroups_HighestAccess()
    {
        var grantPolicies = new GrantCollection()
        {
            new GrantPolicy("customer1", RolePolicy.Reader | RolePolicy.SecurityGroup, "group1"),
            new GrantPolicy("customer1", RolePolicy.Owner | RolePolicy.SecurityGroup, "group2"),
        };
        
        var groupPolicies = new[]
        {
            new GroupPolicy("group1", ["user1"]),
            new GroupPolicy("group2", ["user1"]),
        };
        
        var principals = new[] { new PrincipalIdentity("user1", "id1", "user1", "user1@domain.com", false) };
        
        var grantControl = new GrantControl(groupPolicies, principals);
        
        // Should get highest access level (Owner)
        new AccessRequest(AccessType.AssignRole, "user1", "customer1")
            .Action(x => grantControl.HasAccess(x, grantPolicies).BeTrue());
    }
}
