using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GrantCollection : ICollection<GrantPolicy>, IEquatable<GrantCollection>
{
    private readonly ConcurrentDictionary<string, IReadOnlyList<GrantPolicy>> _data;
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);

    public GrantCollection() => _data = new();

    [JsonConstructor]
    public GrantCollection(IReadOnlyList<GrantPolicy> policies) => _data = policies.NotNull()
        .ForEach(x => x.Validate().ThrowOnError())
        .GroupBy(x => x.NameIdentifier)
        .Select(g => new KeyValuePair<string, IReadOnlyList<GrantPolicy>>(g.Key, g.ToImmutableArray()))
        .ToConcurrentDictionary();


    public int Count => _data.Values.Sum(x => x.Count);
    public bool IsReadOnly => false;
    public void Clear() => _data.Clear();
    public bool Contains(GrantPolicy item) => _data.TryGetValue(item.NameIdentifier, out var list) && list.Contains(item);


    public void Add(GrantPolicy item)
    {
        item.Validate().ThrowOnError();

        _lock.EnterReadLock();

        try
        {
            _data.AddOrUpdate(
                item.NameIdentifier,
                ImmutableArray.Create(item),
                (_, existing) => ((ImmutableArray<GrantPolicy>)existing).Add(item)
            );
        }
        finally { _lock.ExitReadLock(); }
    }

    /// If a group principal has access, returns list of group names that have access if forbidden is returned
    /// 
    public Option<IReadOnlyList<string>> InPolicy(AccessRequest securityRequest)
    {
        if (!_data.TryGetValue(securityRequest.NameIdentifier, out var policies) || policies is null)
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

    public bool Remove(GrantPolicy item)
    {
        _lock.EnterWriteLock();

        try
        {
            if (!_data.TryGetValue(item.NameIdentifier, out var existing)) return false;

            var updated = ((ImmutableArray<GrantPolicy>)existing).Remove(item);
            if (updated.IsEmpty) return _data.TryRemove(item.NameIdentifier, out _);

            _data[item.NameIdentifier] = updated;
            return true;
        }
        finally { _lock.ExitWriteLock(); }
    }

    public void CopyTo(GrantPolicy[] array, int arrayIndex)
    {
        array.NotNull();
        if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        foreach (var policy in _data.Values.SelectMany(x => x))
        {
            if (arrayIndex >= array.Length) throw new ArgumentException("Destination array is not long enough.");
            array[arrayIndex++] = policy;
        }
    }

    public IEnumerator<GrantPolicy> GetEnumerator() => _data.Values.SelectMany(x => x).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool TryGetValue(string nameIdentifier, out IReadOnlyList<GrantPolicy>? grants) =>
        _data.TryGetValue(nameIdentifier, out grants);

    public override bool Equals(object? obj) => obj is GrantCollection other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(_data);

    public bool Equals(GrantCollection? other) => other is not null &&
        _data.Count == other._data.Count &&
        _data.All(pair =>
            other._data.TryGetValue(pair.Key, out var otherPolicies) &&
            pair.Value.SequenceEqual(otherPolicies)
        );

    public static bool operator ==(GrantCollection? left, GrantCollection? right) => left?.Equals(right) ?? false;
    public static bool operator !=(GrantCollection? left, GrantCollection? right) => !left?.Equals(right) ?? false;
}
