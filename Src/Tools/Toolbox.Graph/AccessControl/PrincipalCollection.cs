//using System.Collections;
//using System.Collections.Concurrent;
//using System.Collections.Immutable;
//using System.Text.Json.Serialization;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph;

//public class PrincipalCollection : IEquatable<PrincipalCollection>, IEnumerable<PrincipalIdentity>
//{
//    private readonly ConcurrentDictionary<string, PrincipalIdentity> _principals = new(StringComparer.OrdinalIgnoreCase);
//    private readonly ConcurrentDictionary<string, PrincipalIdentity> _nameIdentityIndex = new(StringComparer.OrdinalIgnoreCase);
//    private readonly ConcurrentDictionary<string, PrincipalIdentity> _emailIndex = new(StringComparer.OrdinalIgnoreCase);
//    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);
//    private readonly GrantControl _grantControl;

//    public PrincipalCollection(GrantControl grantControl) => _grantControl = grantControl;

//    [JsonConstructor]
//    public PrincipalCollection(IEnumerable<PrincipalIdentity> principals, GrantControl grantControl)
//    {
//        var items = principals.NotNull().ToArray();
//        foreach (var p in items) p.Validate().ThrowOnError();
//        _grantControl = grantControl;

//        // Build both indices from the same validated snapshot to avoid re-enumeration races.
//        _principals = items.ToConcurrentDictionary(x => x.PrincipalId, StringComparer.OrdinalIgnoreCase);
//        _nameIdentityIndex = items.ToConcurrentDictionary(x => x.NameIdentifier, StringComparer.OrdinalIgnoreCase);
//        _emailIndex = items.ToConcurrentDictionary(x => x.Email, StringComparer.OrdinalIgnoreCase);
//    }

//    public int Count => _principals.Count;
//    public bool TryGetValue(string key, out PrincipalIdentity? subject)
//    {
//        _lock.EnterReadLock();
//        try { return _principals.TryGetValue(key.NotEmpty(), out subject); }
//        finally { _lock.ExitReadLock(); }
//    }

//    public bool TryGetByNameIdentifier(string key, out PrincipalIdentity? subject)
//    {
//        _lock.EnterReadLock();
//        try { return _nameIdentityIndex.TryGetValue(key.NotEmpty(), out subject); }
//        finally { _lock.ExitReadLock(); }
//    }

//    public bool TryGetByEmail(string key, out PrincipalIdentity? subject)
//    {
//        _lock.EnterReadLock();
//        try { return _emailIndex.TryGetValue(key.NotEmpty(), out subject); }
//        finally { _lock.ExitReadLock(); }
//    }

//    public void Clear()
//    {
//        _lock.EnterWriteLock();
//        try
//        {
//            _principals.Clear();
//            _nameIdentityIndex.Clear();
//            _emailIndex.Clear();
//        }
//        finally { _lock.ExitWriteLock(); }
//    }

//    public bool Contains(string principalId)
//    {
//        _lock.EnterReadLock();
//        try { return _principals.ContainsKey(principalId.NotEmpty()); }
//        finally { _lock.ExitReadLock(); }
//    }

//    /// <summary>
//    /// Adds a principal identity to the collection, ensuring that the name identifier and email are unique within the
//    /// collection.
//    /// </summary>
//    /// <remarks>This method enforces uniqueness for both the name identifier and email of each principal. If
//    /// a principal with the same key already exists, its name identifier and email are updated only if they remain
//    /// unique. Validation is performed on the principal before it is added.</remarks>
//    /// <param name="principal">The principal identity to add. Must not be null and must pass validation.</param>
//    /// <param name="trxContext">An optional transaction context used to manage the operation's state. If not specified, the default context is
//    /// used.</param>
//    /// <exception cref="ArgumentException">Thrown if the name identifier or email of the principal already exists in the collection.</exception>
//    public void Set(PrincipalIdentity principal, GraphTrxContext? trxContext = null)
//    {
//        principal.NotNull().Validate().ThrowOnError();

//        _lock.EnterWriteLock();
//        try
//        {
//            if (_principals.TryGetValue(principal.PrincipalId, out var existing))
//            {
//                if (principal != existing) updateIndexes(existing);
//                return;
//            }

//            addIndexes(principal);
//        }
//        finally { _lock.ExitWriteLock(); }


//        void addIndexes(PrincipalIdentity principal)
//        {
//            _nameIdentityIndex.ContainsKey(principal.NameIdentifier).Assert(x => x == false, "NameIdentifier already exist");
//            _emailIndex.ContainsKey(principal.Email).Assert(x => x == false, "Email already exist");

//            _principals.TryAdd(principal.PrincipalId, principal).BeTrue($"Principal {principal.PrincipalId} already exist");
//            _nameIdentityIndex.TryAdd(principal.NameIdentifier, principal).BeTrue($"Failed to add nameIdentifier={principal.NameIdentifier}");
//            _emailIndex.TryAdd(principal.Email, principal).BeTrue($"Failed to add email={principal.Email}");

//            trxContext?.Recorder?.Add(principal.PrincipalId, principal);
//        }

//        void updateIndexes(PrincipalIdentity existing)
//        {
//            existing.PrincipalId.EqualsIgnoreCase(principal.PrincipalId).BeTrue($"existing.Key{existing.PrincipalId} does not match updated.Key={principal.PrincipalId}");

//            if (!existing.NameIdentifier.EqualsIgnoreCase(principal.NameIdentifier))
//            {
//                _nameIdentityIndex.ContainsKey(principal.NameIdentifier).Assert(x => x == false, "NameIdentifier already exist");
//                _nameIdentityIndex.TryRemove(existing.NameIdentifier, out _).BeTrue($"Failed to remove {existing.NameIdentifier}");
//            }

//            if (!existing.Email.EqualsIgnoreCase(principal.Email))
//            {
//                _emailIndex.ContainsKey(principal.Email).Assert(x => x == false, "Email already exist");
//                _emailIndex.TryRemove(existing.Email, out _).BeTrue($"Failed to remove {existing.Email}");
//            }

//            _principals[principal.PrincipalId] = principal;
//            _nameIdentityIndex[principal.NameIdentifier] = principal;
//            _emailIndex[principal.Email] = principal;

//            trxContext?.Recorder?.Update(principal.PrincipalId, existing, principal);
//        }
//    }

//    internal Option Remove(string principalId, GraphTrxContext? trxContext)
//    {
//        principalId.NotEmpty();

//        _lock.EnterWriteLock();
//        try
//        {
//            var removed = _principals.TryRemove(principalId, out var principalIdentity);
//            if (!removed || principalIdentity is null) return (StatusCode.NotFound, $"Principal with ID '{principalId}' not found.");

//            _nameIdentityIndex.TryRemove(principalIdentity.NameIdentifier, out var _).BeTrue($"Failed to remove {principalIdentity.NameIdentifier}");
//            _emailIndex.TryRemove(principalIdentity.Email, out _).BeTrue($"Failed to remove {principalIdentity.Email}");

//            //trxContext?.Recorder?.Delete(principalIdentity.Key, principalId);
//            return StatusCode.OK;
//        }
//        finally { _lock.ExitWriteLock(); }
//    }

//    public IEnumerator<PrincipalIdentity> GetEnumerator()
//    {
//        _lock.EnterReadLock();
//        try
//        {
//            var snapShot = _principals.Values.ToList();
//            return snapShot.GetEnumerator();
//        }
//        finally { _lock.ExitReadLock(); }
//    }

//    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

//    public override bool Equals(object? obj) => obj is PrincipalCollection other && Equals(other);

//    public bool Equals(PrincipalCollection? other)
//    {
//        if (ReferenceEquals(this, other)) return true;
//        if (other is null) return false;

//        // Snapshot our entries once; compare against other's live view.
//        var ours = _principals.ToArray();
//        if (other.Count != ours.Length) return false;

//        foreach (var pair in ours)
//        {
//            if (!other._principals.TryGetValue(pair.Key, out var otherValue)) return false;
//            if (pair.Value != otherValue) return false;
//        }

//        return true;
//    }

//    public override int GetHashCode()
//    {
//        // Order-independent, content-based hash. Snapshot to avoid races.
//        var snapshot = _principals.ToArray();

//        var acc = 0;
//        foreach (var kv in snapshot)
//        {
//            // Combine key and value hash; XOR makes it order-independent.
//            acc ^= HashCode.Combine(kv.Key, kv.Value);
//        }

//        // Include count to distinguish empty/non-empty with same XOR.
//        return HashCode.Combine(acc, snapshot.Length);
//    }

//    public static bool operator ==(PrincipalCollection? left, PrincipalCollection? right) =>
//        ReferenceEquals(left, right) || (left is not null && left.Equals(right));

//    public static bool operator !=(PrincipalCollection? left, PrincipalCollection? right) => !(left == right);
//}
