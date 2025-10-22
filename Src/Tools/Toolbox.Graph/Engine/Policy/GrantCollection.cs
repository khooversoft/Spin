//using System.Collections;
//using System.Collections.Concurrent;
//using System.Collections.Immutable;
//using System.Text.Json.Serialization;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph;

//public class GrantCollection : ICollection<GrantPolicy>, IEquatable<GrantCollection>
//{
//    private ImmutableArray<GrantPolicy> _grantPolicies = ImmutableArray<GrantPolicy>.Empty;
//    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);

//    public GrantCollection() { }

//    [JsonConstructor]
//    public GrantCollection(IReadOnlyList<GrantPolicy> policies)
//    {
//        if (policies.Count == 0)
//        {
//            _grantPolicies = ImmutableArray<GrantPolicy>.Empty;
//            return;
//        }

//        var seen = new HashSet<GrantPolicy>(policies.Count);
//        var builder = ImmutableArray.CreateBuilder<GrantPolicy>(policies.Count);

//        for (int i = 0; i < policies.Count; i++)
//        {
//            var p = policies[i];
//            if (seen.Add(p)) builder.Add(p);
//        }

//        _grantPolicies = builder.MoveToImmutable();
//    }

//    public int Count => _grantPolicies.Length;
//    public bool IsReadOnly => false;
//    public void Clear() => _grantPolicies = _grantPolicies.Clear();
//    public bool Contains(GrantPolicy item) => _grantPolicies.Contains(item);

//    public void Add(GrantPolicy item)
//    {
//        item.Validate().ThrowOnError();

//        _lock.EnterReadLock();

//        try
//        {
//            var current = _grantPolicies; // capture snapshot
//            _grantPolicies = current.Any(x => x == item) ? current : current.Add(item);
//        }
//        finally { _lock.ExitReadLock(); }
//    }

//    /// If a group principal has access, returns list of group names that have access if forbidden is returned
//    /// 
//    public Option<IReadOnlyList<string>> InPolicy(AccessRequest securityRequest)
//    {
//        var policies = _grantPolicies; // capture snapshot
//        if (policies.Length == 0) return StatusCode.OK;

//        var span = policies.AsSpan();
//        var groupList = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

//        bool nameIdentifierFound = false;
//        foreach (ref readonly var principal in span)
//        {
//            // grants for the right name identifier
//            if (principal.NameIdentifier!= securityRequest.NameIdentifier) continue;
//            nameIdentifierFound = true;

//            var accessType = principal.Role.ToAccessType();
//            if ((accessType & securityRequest.AccessType) == 0) continue;

//            if ((principal.Role & RolePolicy.SecurityGroup) != 0)
//            {
//                groupList.Add(principal.PrincipalIdentifier);
//                continue;
//            }

//            if (principal.PrincipalIdentifier == securityRequest.PrincipalIdentifier) return StatusCode.OK;
//        }

//        switch(groupList.Count, nameIdentifierFound)
//        {
//            case (0, false): return StatusCode.OK; // name identifier not found
//            case (0, true): return StatusCode.Unauthorized; // name identifier found, but no access
//        }

//        var result = ImmutableArray.CreateRange(groupList);
//        return new Option<IReadOnlyList<string>>(result, StatusCode.NotFound);
//    }

//    public bool Remove(GrantPolicy item)
//    {
//        _lock.EnterWriteLock();

//        try
//        {
//            var current = _grantPolicies; // capture snapshot
//            var updated = current.Remove(item); // removes first match using equality

//            if (!updated.Equals(current)) // compare snapshots explicitly
//            {
//                _grantPolicies = updated;
//                return true;
//            }

//            return false;
//        }
//        finally { _lock.ExitWriteLock(); }
//    }

//    public void CopyTo(GrantPolicy[] array, int arrayIndex)
//    {
//        array.NotNull();
//        if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

//        foreach (var policy in _grantPolicies)
//        {
//            if (arrayIndex >= array.Length) throw new ArgumentException("Destination array is not long enough.");
//            array[arrayIndex++] = policy;
//        }
//    }

//    public IEnumerator<GrantPolicy> GetEnumerator() => ((IEnumerable<GrantPolicy>)_grantPolicies).GetEnumerator();
//    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

//    public bool TryGetValue(string principalIdentifier, out IReadOnlyList<GrantPolicy>? grants)
//    {
//        grants = [.. _grantPolicies.Where(x => x.PrincipalIdentifier == principalIdentifier)];
//        return grants.Any();
//    }

//    public override bool Equals(object? obj) => obj is GrantCollection other && Equals(other);
//    public override int GetHashCode() => HashCode.Combine(_grantPolicies);

//    public bool Equals(GrantCollection? other) => other is not null &&
//        _grantPolicies.Length == other._grantPolicies.Length &&
//        _grantPolicies.SequenceEqual(other._grantPolicies);

//    public static bool operator ==(GrantCollection? left, GrantCollection? right) => left?.Equals(right) ?? false;
//    public static bool operator !=(GrantCollection? left, GrantCollection? right) => !left?.Equals(right) ?? false;
//}
