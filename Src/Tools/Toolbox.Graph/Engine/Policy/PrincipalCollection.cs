﻿using System.Collections;
using System.Collections.Concurrent;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class PrincipalCollection : IEquatable<PrincipalCollection>, ICollection<PrincipalIdentity>
{
    private readonly ConcurrentDictionary<string, PrincipalIdentity> _principals = new();
    private readonly ConcurrentDictionary<string, PrincipalIdentity> _nameIdentityIndex = new();
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);

    public PrincipalCollection() { }

    public PrincipalCollection(IEnumerable<PrincipalIdentity> principals)
    {
        var items = principals.NotNull().ToArray();
        foreach (var p in items) p.Validate().ThrowOnError();

        // Build both indices from the same validated snapshot to avoid re-enumeration races.
        _principals = items.ToConcurrentDictionary(x => x.PrincipalId);
        _nameIdentityIndex = items.ToConcurrentDictionary(x => x.NameIdentifier);
    }

    public int Count => _principals.Count;

    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            _principals.Clear();
            _nameIdentityIndex.Clear();
        }
        finally { _lock.ExitWriteLock(); }
    }

    public bool IsReadOnly => false;

    public bool Contains(string principalId) => _principals.ContainsKey(principalId);

    public bool Contains(PrincipalIdentity item) =>
        _principals.TryGetValue(item.NotNull().PrincipalId, out var existing) && existing == item;

    public bool TryGetValue(string principalId, out PrincipalIdentity principalIdentity) =>
        _principals.TryGetValue(principalId.NotEmpty(), out principalIdentity!);

    public bool TryGetByNameIdentifier(string nameIdentifier, out PrincipalIdentity principalIdentity) =>
        _nameIdentityIndex.TryGetValue(nameIdentifier.NotEmpty(), out principalIdentity!);

    public void Add(PrincipalIdentity principal)
    {
        principal.Validate().ThrowOnError();

        _lock.EnterWriteLock();
        try
        {
            // If replacing an existing principal with a different NameIdentifier, remove the old name index entry.
            if (_principals.TryGetValue(principal.PrincipalId, out var existing) && existing.NameIdentifier != principal.NameIdentifier)
            {
                _nameIdentityIndex.TryRemove(existing.NameIdentifier, out _);
            }

            _principals[principal.PrincipalId] = principal;
            _nameIdentityIndex[principal.NameIdentifier] = principal;
        }
        finally { _lock.ExitWriteLock(); }
    }

    public bool Remove(PrincipalIdentity item) => Remove(item.NotNull().PrincipalId);

    public bool Remove(string principalId)
    {
        principalId.NotEmpty();

        _lock.EnterWriteLock();
        try
        {
            var removed = _principals.TryRemove(principalId, out var principalIdentity);
            if (!removed || principalIdentity is null) return false;

            _nameIdentityIndex.TryRemove(principalIdentity.NameIdentifier, out _);
            return true;
        }
        finally { _lock.ExitWriteLock(); }
    }

    public IEnumerator<PrincipalIdentity> GetEnumerator() => _principals.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void CopyTo(PrincipalIdentity[] array, int arrayIndex)
    {
        array.NotNull();
        if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        // Snapshot to avoid partial copies and race with writers.
        var snapshot = _principals.Values.ToArray();

        if (array.Length - arrayIndex < snapshot.Length)
            throw new ArgumentException("Destination array is not long enough.");

        Array.Copy(snapshot, 0, array, arrayIndex, snapshot.Length);
    }

    public override bool Equals(object? obj) => obj is PrincipalCollection other && Equals(other);

    public bool Equals(PrincipalCollection? other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other is null) return false;

        // Snapshot our entries once; compare against other's live view.
        var ours = _principals.ToArray();
        if (other.Count != ours.Length) return false;

        foreach (var pair in ours)
        {
            if (!other._principals.TryGetValue(pair.Key, out var otherValue)) return false;
            if (pair.Value != otherValue) return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        // Order-independent, content-based hash. Snapshot to avoid races.
        var snapshot = _principals.ToArray();

        var acc = 0;
        foreach (var kv in snapshot)
        {
            // Combine key and value hash; XOR makes it order-independent.
            acc ^= HashCode.Combine(kv.Key, kv.Value);
        }

        // Include count to distinguish empty/non-empty with same XOR.
        return HashCode.Combine(acc, snapshot.Length);
    }

    public static bool operator ==(PrincipalCollection? left, PrincipalCollection? right) =>
        ReferenceEquals(left, right) || (left is not null && left.Equals(right));

    public static bool operator !=(PrincipalCollection? left, PrincipalCollection? right) => !(left == right);
}
