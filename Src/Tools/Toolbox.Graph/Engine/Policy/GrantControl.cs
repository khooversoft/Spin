using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

/// <summary>
/// Policies that give security group or user roles such as read, contributor, owner to a node or edge in the graph
/// 
/// Security groups - list or user that have the same access
/// 
/// </summary>
public class GrantControl : IEquatable<GrantControl>
{
    public GrantControl() { }

    public GrantControl(IReadOnlyList<GrantPolicy> grantPolicies, IReadOnlyList<GroupPolicy> securityGroups)
    {
        Policies = new GrantCollection(grantPolicies);
        Groups = new GroupCollection(securityGroups);
    }

    public GrantCollection Policies { get; init; } = new();
    public GroupCollection Groups { get; init; } = new();

    public bool HasAccess(AccessRequest securityRequest)
    {
        var policyOption = Policies.InPolicy(securityRequest);
        if (policyOption.IsOk() || policyOption.IsNotFound()) return true;

        // Returns a list of group names that are in policy
        var groupNames = policyOption.Return();
        return groupNames.Any(x => Groups.InGroup(x, securityRequest.PrincipalIdentifier));
    }

    public override bool Equals(object? obj) => obj is GrantControl other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Policies, Groups);

    public bool Equals(GrantControl? other) => other is not null &&
        Policies == other.Policies &&
        Groups == other.Groups;

    public static bool operator ==(GrantControl? left, GrantControl? right) => left?.Equals(right) ?? false;
    public static bool operator !=(GrantControl? left, GrantControl? right) => !left?.Equals(right) ?? false;
}
