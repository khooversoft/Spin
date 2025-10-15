using Toolbox.Extensions;
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

    public GrantControl(IReadOnlyList<GroupPolicy> securityGroups, IReadOnlyList<PrincipalIdentity> principalIdentities)
    {
        Principals = [.. principalIdentities];
        Groups = [.. securityGroups];
    }

    public PrincipalCollection Principals { get; init; } = new();
    public GroupCollection Groups { get; init; } = new();

    public bool HasAccess(AccessRequest securityRequest, GrantCollection grantCollections)
    {
        // Check if user exist
        if (!Principals.Contains(securityRequest.PrincipalIdentifier)) return false;

        // Check if user is directly in policy
        var groupsOption = grantCollections.InPolicy(securityRequest);
        if (groupsOption.IsOk()) return true;
        if (groupsOption.IsUnauthorized()) return false;

        // User not found, check to see if user is in a group that is in policy
        var groupNames = groupsOption.Return();
        return groupNames.Any(x => Groups.InGroup(x, securityRequest.PrincipalIdentifier));
    }

    public override bool Equals(object? obj) => obj is GrantControl other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Principals, Groups);

    public bool Equals(GrantControl? other) => other is not null &&
        Principals == other.Principals &&
        Groups == other.Groups;

    public static bool operator ==(GrantControl? left, GrantControl? right) => left?.Equals(right) ?? false;
    public static bool operator !=(GrantControl? left, GrantControl? right) => !left?.Equals(right) ?? false;
}
