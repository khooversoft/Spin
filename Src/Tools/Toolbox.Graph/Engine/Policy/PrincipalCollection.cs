using System.Collections;
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
        _principals = principals.NotNull()
            .ForEach(x => x.Validate().ThrowOnError())
            .ToConcurrentDictionary(x => x.PrincipalId);

        _nameIdentityIndex = principals.NotNull()
            .ForEach(x => x.Validate().ThrowOnError())
            .ToConcurrentDictionary(x => x.NameIdentifier);
    }

    public int Count => _principals.Count;
    public void Clear() => _principals.Clear();
    public bool IsReadOnly => false;
    public bool Contains(string principalId) => _principals.ContainsKey(principalId);
    public bool Contains(PrincipalIdentity item) => _principals.TryGetValue(item.PrincipalId, out var existing) && existing == item;
    public bool TryGetValue(string principalId, out PrincipalIdentity principalIdentity) => _principals.TryGetValue(principalId, out principalIdentity!);
    public bool TryGetByNameIdentifier(string nameIdentifier, out PrincipalIdentity principalIdentity) => _nameIdentityIndex.TryGetValue(nameIdentifier, out principalIdentity!);


    public void Add(PrincipalIdentity principal)
    {
        principal.Validate().ThrowOnError();

        _lock.EnterWriteLock();

        try
        {
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
            var result = _principals.TryRemove(principalId, out var principalIdentity);
            if (!result || principalIdentity is null) return false;

            result = _nameIdentityIndex.TryRemove(principalIdentity.NameIdentifier, out _);
            return result;
        }
        finally { _lock.ExitWriteLock(); }
    }

    public IEnumerator<PrincipalIdentity> GetEnumerator() => _principals.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void CopyTo(PrincipalIdentity[] array, int arrayIndex)
    {
        array.NotNull();
        if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        foreach (var item in _principals.Values)
        {
            if (arrayIndex >= array.Length) throw new ArgumentException("Destination array is not long enough.");
            array[arrayIndex++] = item;
        }
    }

    public override bool Equals(object? obj) => obj is PrincipalCollection other && Equals(other);

    public bool Equals(PrincipalCollection? other)
    {
        var result = other is not null &&
            _principals.Count == other._principals.Count &&
            _principals.All(pair =>
            {
                var t1 = other._principals.TryGetValue(pair.Key, out var otherValue);
                var t2 = pair.Value == otherValue;
                return t1 && t2;
            });

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(_principals);

    public static bool operator ==(PrincipalCollection? left, PrincipalCollection? right) => left?.Equals(right) ?? false;
    public static bool operator !=(PrincipalCollection? left, PrincipalCollection? right) => !left?.Equals(right) ?? false;
}
