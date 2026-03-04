using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public enum RolePolicy
{
    None,
    Reader,
    Contributor,
    Owner,
}

/// <summary>
/// Specify that a user or group (identified by the principal identifier) has
/// a specific role (reader, contributor, owner) on a node in the graph.
/// </summary>
public record PrincipalGrant
{
    [JsonConstructor]
    public PrincipalGrant(RolePolicy role, string principalIdentifier)
    {
        Role = role.Assert(x => x != RolePolicy.None, x => $"Invalid role={role}");
        PrincipalIdentifier = principalIdentifier.NotEmpty();
    }

    public RolePolicy Role { get; }
    public string PrincipalIdentifier { get; }
}

/// <summary>
/// Records the grant polcieis for a single node in the graph.
/// Each policy is for a single name identifier, and contains a list of principal identifiers
/// that have access to the node, and the role that they have.
/// </summary>
public sealed record GrantPolicy : IEquatable<GrantPolicy>
{
    public const string NodeType = "grantPolicy";
    public const string EdgeType = "hasGrantPolicy";
    private readonly FrozenDictionary<string, PrincipalGrant> _grants;

    public GrantPolicy(string nodeKey, IEnumerable<PrincipalGrant> principalIdentifiers)
        : this(nodeKey, principalIdentifiers.NotNull().ToDictionary(x => x.PrincipalIdentifier))
    {
    }

    [JsonConstructor]
    public GrantPolicy(string nodeKey, IReadOnlyDictionary<string, PrincipalGrant> principalIdentifiers)
    {
        principalIdentifiers.NotNull();

        NodeKey = NodeTool.CreateKey(nodeKey, NodeType);
        _grants = principalIdentifiers.ToFrozenDictionary(x => x.Key, x => x.Value);
    }

    public string NodeKey { get; }
    public IReadOnlyDictionary<string, PrincipalGrant> PrincipalIdentifiers => _grants;

    public GrantPolicy AddOrUpdateWith(RolePolicy role, params IEnumerable<string> principalIdentifiers)
    {
        principalIdentifiers.NotNull().ForEach(x => x.NotEmpty());

        var updatedSet = _grants.Values
            .Where(x => !principalIdentifiers.Contains(x.PrincipalIdentifier, StringComparer.OrdinalIgnoreCase))
            .Concat(principalIdentifiers.Select(x => new PrincipalGrant(role, x)))
            .ToArray();

        return new GrantPolicy(NodeKey, updatedSet);
    }

    public GrantPolicy RemoveWith(params IEnumerable<string> principalIdentifiers)
    {
        principalIdentifiers.NotNull();

        var newSet = _grants.Values
            .Where(x => !principalIdentifiers.Contains(x.PrincipalIdentifier, StringComparer.OrdinalIgnoreCase))
            .ToArray();

        return new GrantPolicy(NodeKey, newSet);
    }


    public bool CanRead(string principalIdentifier) => GetRole(principalIdentifier) != RolePolicy.None;

    public bool CanWrite(string principalIdentifier) => GetRole(principalIdentifier) switch
    {
        RolePolicy.Contributor => true,
        RolePolicy.Owner => true,
        _ => false,
    };

    public bool IsOwner(string principalIdentifier) => GetRole(principalIdentifier) == RolePolicy.Owner;

    public RolePolicy GetRole(string principalIdentifier)
    {
        principalIdentifier.NotEmpty();
        if (_grants.TryGetValue(principalIdentifier, out var grant)) return grant.Role;
        return RolePolicy.None;
    }

    public bool Equals(GrantPolicy? other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other is null) return false;

        if (!string.Equals(NodeKey, other.NodeKey, StringComparison.Ordinal)) return false;
        if (_grants.Count != other._grants.Count) return false;

        foreach (var item in _grants.Values)
        {
            if (!other._grants.TryGetValue(item.PrincipalIdentifier, out var otherGrant)) return false;
            if (!item.Equals(otherGrant)) return false;
        }

        return true;
    }

    public override int GetHashCode() => HashCode.Combine(NodeKey, _grants.GetHashCode());

    public static IValidator<GrantPolicy> Validator { get; } = new Validator<GrantPolicy>()
        .RuleFor(x => x.NodeKey).NotEmpty()
        .RuleFor(x => x.PrincipalIdentifiers).NotNull()
        .Build();

    public static string CreateKey(string nodeKey) => $"{NodeType}/{nodeKey}".ToLowerInvariant();
}

public static class GrantPolicyTool
{
    public static Option Validate(this GrantPolicy subject) => GrantPolicy.Validator.Validate(subject).ToOptionStatus();
}
