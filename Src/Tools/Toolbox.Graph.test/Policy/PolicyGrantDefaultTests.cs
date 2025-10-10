using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph.test.Policy;

public class PolicyGrantDefaultTests
{
    [Fact]
    public void GrantPolicy_Defaults()
    {
        var grantPolicies = new[]
        {
            new GrantPolicy("customer1", RolePolicy.Owner | RolePolicy.NameIdentifier, "user1orGroupName"),
            new GrantPolicy("customer2", RolePolicy.Reader | RolePolicy.SecurityGroup, "user1orGroupName2"),
        };

        var groupPolicies = new[]
        {
            new GroupPolicy("group1", new[] { "user1", "user2" }),
            new GroupPolicy("group2", new[] { "user3", "user4" }),
        };

        var grantControl = new GrantControl(grantPolicies, groupPolicies);

        // Not protected, should have access
        var request = new AccessRequest(AccessType.Get, "user1", "customer3");

        grantControl.HasAccess(request).BeTrue();
    }

    [Fact]
    public void EqualNotEqual()
    {
        var v1 = new GrantPolicy("customer1", RolePolicy.Owner | RolePolicy.NameIdentifier, "user1orGroupName");
        var v2 = new GrantPolicy("customer1", RolePolicy.Owner | RolePolicy.NameIdentifier, "user1orGroupName");
        (v1 == v2).BeTrue();
        (v1 != v2).BeFalse();

        var v3 = new GrantPolicy("customer12", RolePolicy.Owner | RolePolicy.NameIdentifier, "user1orGroupName");
        (v1 == v3).BeFalse();
        (v1 != v3).BeTrue();

        var v4 = new GrantPolicy("customer1", RolePolicy.Contributor | RolePolicy.NameIdentifier, "user1orGroupName");
        (v1 == v3).BeFalse();

        var v5 = new GrantPolicy("customer1", RolePolicy.Owner | RolePolicy.NameIdentifier, "user1orGroupName-bad");
        (v1 == v5).BeFalse();
    }

    [Fact]
    public void GrantPolicy_Validate_GetAccess()
    {
        var grantPolicies = new[]
        {
            new GrantPolicy("customer1", RolePolicy.Reader | RolePolicy.NameIdentifier, "user1"),
            new GrantPolicy("customer1", RolePolicy.Contributor | RolePolicy.NameIdentifier, "user2"),
            new GrantPolicy("customer1", RolePolicy.Owner | RolePolicy.NameIdentifier, "user3"),
        };

        var grantControl = new GrantControl(grantPolicies, []);

        // Protected, but no access
        new AccessRequest(AccessType.Get, "user_no", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.Create, "user_no", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.Update, "user_no", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.Delete, "user_no", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.AssignRole, "user_no", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());

        // Protected, should have access
        new AccessRequest(AccessType.Get, "user1", "customer1").Action(x => grantControl.HasAccess(x).BeTrue());
        new AccessRequest(AccessType.Create, "user1", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.Update, "user1", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.Delete, "user1", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.AssignRole, "user1", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());

        // protected, but does not should have access
        new AccessRequest(AccessType.Get, "user2", "customer1").Action(x => grantControl.HasAccess(x).BeTrue());
        new AccessRequest(AccessType.Create, "user2", "customer1").Action(x => grantControl.HasAccess(x).BeTrue());
        new AccessRequest(AccessType.Update, "user2", "customer1").Action(x => grantControl.HasAccess(x).BeTrue());
        new AccessRequest(AccessType.Delete, "user2", "customer1").Action(x => grantControl.HasAccess(x).BeTrue());
        new AccessRequest(AccessType.AssignRole, "user2", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());

        // protected, but does not should have access
        new AccessRequest(AccessType.Get, "user3", "customer1").Action(x => grantControl.HasAccess(x).BeTrue());
        new AccessRequest(AccessType.Create, "user3", "customer1").Action(x => grantControl.HasAccess(x).BeTrue());
        new AccessRequest(AccessType.Update, "user3", "customer1").Action(x => grantControl.HasAccess(x).BeTrue());
        new AccessRequest(AccessType.Delete, "user3", "customer1").Action(x => grantControl.HasAccess(x).BeTrue());
        new AccessRequest(AccessType.AssignRole, "user3", "customer1").Action(x => grantControl.HasAccess(x).BeTrue());
    }

    [Fact]
    public void GrantPolicyGroup_Validate_GetAccess()
    {
        var grantPolicies = new[]
        {
            new GrantPolicy("customer1", RolePolicy.Reader | RolePolicy.SecurityGroup, "group1"),
            new GrantPolicy("customer1", RolePolicy.Contributor | RolePolicy.SecurityGroup, "group2"),
            new GrantPolicy("customer1", RolePolicy.Owner | RolePolicy.SecurityGroup, "group3"),
        };

        var groupPolicies = new[]
        {
            new GroupPolicy("group1", new[] { "user1", "user9" }),
            new GroupPolicy("group2", new[] { "user2", "user5" }),
            new GroupPolicy("group3", new[] { "user3", "user5" }),
            new GroupPolicy("group4", new[] { "user4", "user6" }),
        };


        var grantControl = new GrantControl(grantPolicies, groupPolicies);

        // Not protected, should have access
        new AccessRequest(AccessType.Get, "user4", "not-protected").Action(x => grantControl.HasAccess(x).BeTrue());

        // Protected, but no access
        new AccessRequest(AccessType.Get, "user4", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.Create, "user4", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.Update, "user4", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.Delete, "user4", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.AssignRole, "user4", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());

        // Protected, but no access
        new AccessRequest(AccessType.Get, "user99", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.Create, "user99", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.Update, "user99", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.Delete, "user99", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.AssignRole, "user99", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());

        // Protected, but no access
        new AccessRequest(AccessType.Get, "user4", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.Create, "user4", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.Update, "user4", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.Delete, "user4", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.AssignRole, "user4", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());

        // Protected, should have access
        new AccessRequest(AccessType.Get, "user1", "customer1").Action(x => grantControl.HasAccess(x).BeTrue());
        new AccessRequest(AccessType.Create, "user1", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.Update, "user1", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.Delete, "user1", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());
        new AccessRequest(AccessType.AssignRole, "user1", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());

        // Protected, should have access
        new AccessRequest(AccessType.Get, "user2", "customer1").Action(x => grantControl.HasAccess(x).BeTrue());
        new AccessRequest(AccessType.Create, "user2", "customer1").Action(x => grantControl.HasAccess(x).BeTrue());
        new AccessRequest(AccessType.Update, "user2", "customer1").Action(x => grantControl.HasAccess(x).BeTrue());
        new AccessRequest(AccessType.Delete, "user2", "customer1").Action(x => grantControl.HasAccess(x).BeTrue());
        new AccessRequest(AccessType.AssignRole, "user2", "customer1").Action(x => grantControl.HasAccess(x).BeFalse());

        // protected, but does not should have access
        new AccessRequest(AccessType.Get, "user3", "customer1").Action(x => grantControl.HasAccess(x).BeTrue());
        new AccessRequest(AccessType.Create, "user3", "customer1").Action(x => grantControl.HasAccess(x).BeTrue());
        new AccessRequest(AccessType.Update, "user3", "customer1").Action(x => grantControl.HasAccess(x).BeTrue());
        new AccessRequest(AccessType.Delete, "user3", "customer1").Action(x => grantControl.HasAccess(x).BeTrue());
        new AccessRequest(AccessType.AssignRole, "user3", "customer1").Action(x => grantControl.HasAccess(x).BeTrue());
    }
}
