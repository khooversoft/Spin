using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GrantCollection : IEquatable<GrantCollection>
{
    private readonly ConcurrentDictionary<string, IReadOnlyList<GrantPolicy>> _policies;
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);

    public GrantCollection() => _policies = new();

    [JsonConstructor]
    public GrantCollection(IReadOnlyList<GrantPolicy> policies) => _policies = policies.NotNull()
        .GroupBy(x => x.NameIdentifier)
        .Select(x => new KeyValuePair<string, IReadOnlyList<GrantPolicy>>(x.Key, x.ToImmutableArray()))
        .ToConcurrentDictionary();

    public IReadOnlyList<GrantPolicy> this[string nameIdentifier]
    {
        get => _policies[nameIdentifier];
        set => _policies[nameIdentifier] = value;
    }

    public IReadOnlyList<GrantPolicy> Policies => _policies.Values.SelectMany(x => x).ToImmutableArray();

    public GrantCollection Add(GrantPolicy policy)
    {
        policy.Validate().ThrowOnError();

        _lock.EnterReadLock();

        try
        {
            _policies.AddOrUpdate(
                policy.NameIdentifier,
                policy.ToEnumerable().ToImmutableArray(),
                (_, existingPolicies) => existingPolicies.Append(policy).ToImmutableArray()
                );
        }
        finally { _lock.ExitWriteLock(); }

        return this;
    }

    public void Clear() => _policies.Clear();

    public bool Remove(GrantPolicy policy)
    {
        _lock.EnterWriteLock();

        try
        {
            if (_policies.TryGetValue(policy.NameIdentifier, out var existingPolicies))
            {
                var updatedPolicies = existingPolicies.Where(x => !x.Equals(policy)).ToImmutableArray();
                if (updatedPolicies.IsEmpty) return _policies.TryRemove(policy.NameIdentifier, out _);

                _policies[policy.NameIdentifier] = updatedPolicies;
                return true;
            }
        }
        finally { _lock.ExitWriteLock(); }

        return false;
    }

    /// If a group principal has access, returns list of group names that have access if forbidden is returned
    /// 
    public Option<IReadOnlyList<string>> InPolicy(AccessRequest securityRequest)
    {
        if (!_policies.TryGetValue(securityRequest.NameIdentifier, out var policies) || policies is null)
        {
            return StatusCode.NotFound;
        }

        // Cast back to ImmutableArray to access AsSpan()
        var immutableArray = (ImmutableArray<GrantPolicy>)policies;
        var span = immutableArray.AsSpan();
        var list = new HashSet<string>();

        foreach (ref readonly var principal in span)
        {
            var accessType = principal.Role.ToAccessType();
            if (!accessType.HasFlag(securityRequest.AccessType)) continue;

            if (principal.Role.HasFlag(RolePolicy.SecurityGroup))
            {
                list.Add(principal.PrincipalIdentifier);
                continue;
            }

            if (principal.PrincipalIdentifier == securityRequest.PrincipalIdentifier) return StatusCode.OK;
        }

        var result = list.ToImmutableArray();
        return new Option<IReadOnlyList<string>>(result, StatusCode.Forbidden);
    }

    public bool TryGetValue(string nameIdentifier, out IReadOnlyList<GrantPolicy>? grants) => _policies.TryGetValue(nameIdentifier, out grants);

    public override bool Equals(object? obj) => obj is GrantCollection other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(_policies);

    public bool Equals(GrantCollection? other) => other is not null &&
        _policies.Count == other._policies.Count &&
        _policies.All(pair =>
                    other._policies.TryGetValue(pair.Key, out var otherPolicies) &&
                    pair.Value.SequenceEqual(otherPolicies)
                );

    public static bool operator ==(GrantCollection? left, GrantCollection? right) => left?.Equals(right) ?? false;
    public static bool operator !=(GrantCollection? left, GrantCollection? right) => !left?.Equals(right) ?? false;
}
