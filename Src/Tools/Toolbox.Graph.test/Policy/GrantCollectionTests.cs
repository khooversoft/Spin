using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Tools;
using Toolbox.Extensions;
using Toolbox.Types;
using Xunit;

namespace Toolbox.Graph.test.Policy;

public class GrantCollectionTests
{
    private static AccessRequest Req(AccessType at, string principal, string name) => new(at, principal, name);

    [Fact]
    public void InPolicy_EmptyCollection_Returns_Ok()
    {
        IReadOnlyCollection<GrantPolicy> grants = Array.Empty<GrantPolicy>();

        var r1 = grants.InPolicy(Req(AccessType.Get, "u1", "res1"));
        r1.IsOk().BeTrue();
    }

    [Fact]
    public void InPolicy_NameIdentifier_NotFound_Returns_Ok()
    {
        IReadOnlyCollection<GrantPolicy> grants = new[]
        {
            new GrantPolicy("resA", RolePolicy.Reader | RolePolicy.PrincipalIdentity, "u1"),
        };

        var r = grants.InPolicy(Req(AccessType.Get, "u1", "resB"));
        r.IsOk().BeTrue();
    }

    [Fact]
    public void InPolicy_Direct_User_Access_All_Roles()
    {
        // Reader: Get only
        IReadOnlyCollection<GrantPolicy> reader = new[]
        {
            new GrantPolicy("res1", RolePolicy.Reader | RolePolicy.PrincipalIdentity, "u1"),
        };
        reader.InPolicy(Req(AccessType.Get, "u1", "res1")).IsOk().BeTrue();
        reader.InPolicy(Req(AccessType.Create, "u1", "res1")).IsOk().BeFalse();

        // Contributor: Get, Create, Update, Delete
        IReadOnlyCollection<GrantPolicy> contributor = new[]
        {
            new GrantPolicy("res1", RolePolicy.Contributor | RolePolicy.PrincipalIdentity, "u1"),
        };
        contributor.InPolicy(Req(AccessType.Get, "u1", "res1")).IsOk().BeTrue();
        contributor.InPolicy(Req(AccessType.Create, "u1", "res1")).IsOk().BeTrue();
        contributor.InPolicy(Req(AccessType.Update, "u1", "res1")).IsOk().BeTrue();
        contributor.InPolicy(Req(AccessType.Delete, "u1", "res1")).IsOk().BeTrue();
        contributor.InPolicy(Req(AccessType.AssignRole, "u1", "res1")).IsOk().BeFalse();

        // Owner: All including AssignRole
        IReadOnlyCollection<GrantPolicy> owner = new[]
        {
            new GrantPolicy("res1", RolePolicy.Owner | RolePolicy.PrincipalIdentity, "u1"),
        };
        owner.InPolicy(Req(AccessType.Get, "u1", "res1")).IsOk().BeTrue();
        owner.InPolicy(Req(AccessType.Create, "u1", "res1")).IsOk().BeTrue();
        owner.InPolicy(Req(AccessType.Update, "u1", "res1")).IsOk().BeTrue();
        owner.InPolicy(Req(AccessType.Delete, "u1", "res1")).IsOk().BeTrue();
        owner.InPolicy(Req(AccessType.AssignRole, "u1", "res1")).IsOk().BeTrue();
    }

    [Fact]
    public void InPolicy_Group_Access_Returns_GroupList_With_NotFound()
    {
        IReadOnlyCollection<GrantPolicy> grants = new[]
        {
            new GrantPolicy("res2", RolePolicy.Reader | RolePolicy.SecurityGroup, "groupR"),
            new GrantPolicy("res2", RolePolicy.Contributor | RolePolicy.SecurityGroup, "groupW"),
        };

        var r = grants.InPolicy(Req(AccessType.Get, "userX", "res2"));
        r.StatusCode.Be(StatusCode.NotFound);
        r.HasValue.BeTrue();

        var groups = r.Return();
        groups.Count.Be(2);
        groups.Contains("groupR").BeTrue();
        groups.Contains("groupW").BeTrue();
    }

    [Fact]
    public void InPolicy_GroupList_Deduplicates_And_Is_CaseInsensitive()
    {
        IReadOnlyCollection<GrantPolicy> grants = new[]
        {
            new GrantPolicy("res2", RolePolicy.Reader | RolePolicy.SecurityGroup, "GroupA"),
            new GrantPolicy("res2", RolePolicy.Reader | RolePolicy.SecurityGroup, "groupa"), // duplicate (case-insensitive)
            new GrantPolicy("res2", RolePolicy.Reader | RolePolicy.SecurityGroup, "GroupB"),
        };

        var r = grants.InPolicy(Req(AccessType.Get, "userX", "res2"));
        r.StatusCode.Be(StatusCode.NotFound);
        var groups = r.Return();

        groups.Count.Be(2);
        groups.Any(x => x.Equals("GroupA", StringComparison.OrdinalIgnoreCase)).BeTrue();
        groups.Any(x => x.Equals("GroupB", StringComparison.OrdinalIgnoreCase)).BeTrue();
    }

    [Fact]
    public void InPolicy_NameIdentifier_Matches_But_Insufficient_Role_Returns_Unauthorized()
    {
        // Only Reader granted; request AssignRole (Owner-only)
        IReadOnlyCollection<GrantPolicy> grants = new[]
        {
            new GrantPolicy("resX", RolePolicy.Reader | RolePolicy.PrincipalIdentity, "u1"),
        };

        var r = grants.InPolicy(Req(AccessType.AssignRole, "u1", "resX"));
        r.StatusCode.Be(StatusCode.Unauthorized);
        r.HasValue.BeFalse();
    }

    [Fact]
    public void InPolicy_Direct_User_Takes_Precedence_Over_GroupList()
    {
        // Direct user has Contributor; there are also group grants
        IReadOnlyCollection<GrantPolicy> grants = new[]
        {
            new GrantPolicy("res1", RolePolicy.Contributor | RolePolicy.PrincipalIdentity, "u1"),
            new GrantPolicy("res1", RolePolicy.Reader | RolePolicy.SecurityGroup, "groupR"),
        };

        var r = grants.InPolicy(Req(AccessType.Create, "u1", "res1"));
        r.IsOk().BeTrue(); // returns OK immediately for direct match
    }

    [Fact]
    public void InPolicy_CaseSensitive_NameIdentifier_Behavior()
    {
        // Implementation compares nameIdentifier with case-sensitive equality
        IReadOnlyCollection<GrantPolicy> grants = new[]
        {
            new GrantPolicy("Res1", RolePolicy.Reader | RolePolicy.PrincipalIdentity, "u1"),
        };

        // Different case -> treated as not found -> OK
        grants.InPolicy(Req(AccessType.Get, "u1", "res1")).IsOk().BeTrue();

        // Exact match -> OK
        grants.InPolicy(Req(AccessType.Get, "u1", "Res1")).IsOk().BeTrue();
    }
}
