using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph;

/// <summary>
/// Policies that give security group or user roles such as read, contributor, owner to a node or edge in the graph
/// 
/// Security groups - list or user that have the same access
/// 
/// Not deserializable
/// </summary>
public class GrantControl : IEquatable<GrantControl>
{
    public GrantControl() { }

    [JsonConstructor]
    public GrantControl(IReadOnlyList<GroupPolicy> securityGroups, IReadOnlyList<PrincipalIdentity> principals)
    {
        Principals = [.. principals];
        Groups = [.. securityGroups];
    }

    public PrincipalCollection Principals { get; init; } = new();
    public GroupCollection Groups { get; init; } = new();

    public bool HasAccess(AccessRequest securityRequest, IReadOnlyCollection<GrantPolicy> grantCollections)
    {
        // Check if user exist
        if (!Principals.Contains(securityRequest.PrincipalIdentifier)) return false;

        // Check if user is directly in policy
        var groupsOption = grantCollections.InPolicy(securityRequest);
        if (groupsOption.IsOk()) return true;
        if (groupsOption.IsUnauthorized()) return false;

        // User not found, check to see if user is in a group that is in policy
        IReadOnlyList<string> groupNames = groupsOption.Return();
        for (int i = 0; i < groupNames.Count; i++)
        {
            if (Groups.InGroup(groupNames[i], securityRequest.PrincipalIdentifier)) return true;
        }

        return false;
    }

    public override bool Equals(object? obj) => obj is GrantControl other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Principals, Groups);

    public bool Equals(GrantControl? other) => other is not null &&
        Principals.Equals(other.Principals) &&
        Groups.Equals(other.Groups);

    public static bool operator ==(GrantControl? left, GrantControl? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(GrantControl? left, GrantControl? right) => !(left == right);
}
