//using System.Collections;
//using System.Collections.Concurrent;
//using Toolbox.Data;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph.AccessControl;

//public class GroupCollection : IEquatable<GroupCollection>
//{
//    private readonly ConcurrentDictionary<string, GroupDetail> _groupDetails = new(StringComparer.OrdinalIgnoreCase);
//    private readonly ConcurrentDictionary<string, GroupPolicy> _policies = new(StringComparer.OrdinalIgnoreCase);
//    private readonly SecondaryIndex<string, string> _groupToPolicyIndex = new(StringComparer.OrdinalIgnoreCase);    // Group -> Policy key
//    private readonly SecondaryIndex<string, string> _userToGroupIndex = new(StringComparer.OrdinalIgnoreCase);      // User -> Group
//    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);
//    private readonly Func<string, bool> _checkUser;

//    public GroupCollection(IEnumerable<GroupDetail> groupDetails, Func<string, bool> checkUser, IEnumerable<GroupPolicy> groupPolicies)
//    {
//        _checkUser = checkUser.NotNull();

//        var details = groupDetails.NotNull().ToArray();
//        foreach (var item in details) item.Validate().ThrowOnError();
//        _groupDetails = details.ToConcurrentDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

//        var policies = groupPolicies.NotNull().ToArray();
//        foreach (var item in policies) InternalSetPolicy(item);
//    }

//    public int GroupCount => _groupDetails.Count;
//    public int PolicyCount => _policies.Count;

//    public void Clear()
//    {
//        _lock.EnterWriteLock();
//        try
//        {
//            _groupDetails.Clear();
//            _policies.Clear();
//            _groupToPolicyIndex.Clear();
//            _userToGroupIndex.Clear();
//        }
//        finally { _lock.ExitWriteLock(); }
//    }

//    public Option Set(GroupDetail detail, GraphTrxContext? trxContext = null)
//    {
//        _lock.EnterWriteLock();
//        try { return InternalSetGroup(detail, trxContext); }
//        finally { _lock.ExitWriteLock(); }
//    }

//    //public Option Set(GroupPolicy policy, GraphTrxContext? trxContext = null)
//    //{
//    //    _lock.EnterWriteLock();
//    //    try { return InternalSetPolicy(policy, trxContext); }
//    //    finally { _lock.ExitWriteLock(); }
//    //}

//    public Option RemoveGroup(string groupName, GraphTrxContext? trxContext = null)
//    {
//        _lock.EnterWriteLock();
//        try { return InternalRemoveGroup(groupName, trxContext); }
//        finally { _lock.ExitWriteLock(); }
//    }

//    public Option RemovePolicy(string policyKey, GraphTrxContext? trxContext = null)
//    {
//        _lock.EnterWriteLock();
//        try { return InternalRemovePolicy(policyKey, trxContext); }
//        finally { _lock.ExitWriteLock(); }
//    }

//    public bool TryGetGroup(string groupName, out GroupDetail? group)
//    {
//        _lock.EnterReadLock();
//        try { return _groupDetails.TryGetValue(groupName.NotEmpty(), out group); }
//        finally { _lock.ExitReadLock(); }
//    }

//    public bool TryGetPolicy(string policyKey, out GroupPolicy? group)
//    {
//        _lock.EnterReadLock();
//        try { return _policies.TryGetValue(policyKey.NotEmpty(), out group); }
//        finally { _lock.ExitReadLock(); }
//    }

//    public bool InGroup(string groupName, string principalIdentifier)
//    {
//        groupName.NotEmpty();
//        principalIdentifier.NotEmpty();

//        var result = _userToGroupIndex.Contains(principalIdentifier, groupName);
//        return result;
//    }


//    public override bool Equals(object? obj) => obj is GroupCollection other && Equals(other);

//    // Order-independent hash code based on content; consistent with Equals
//    public override int GetHashCode()
//    {
//        var policySnapshot = _policies.ToArray();
//        var groupSnapshot = _groupDetails.ToArray();

//        var policyHash = 0;
//        foreach (var pair in policySnapshot)
//        {
//            policyHash ^= HashCode.Combine(pair.Key, pair.Value);
//        }

//        var groupHash = 0;
//        foreach (var pair in groupSnapshot)
//        {
//            groupHash ^= HashCode.Combine(pair.Key, pair.Value);
//        }

//        return HashCode.Combine(policyHash, policySnapshot.Length, groupHash, groupSnapshot.Length);
//    }

//    public bool Equals(GroupCollection? other)
//    {
//        if (other == null) return false;
//        if (ReferenceEquals(this, other)) return true;

//        var oursPolicies = _policies.ToArray();
//        if (other._policies.Count != oursPolicies.Length) return false;

//        foreach (var item in oursPolicies)
//        {
//            if (!other._policies.TryGetValue(item.Key, out var otherPolicy)) return false;
//            if (item.Value != otherPolicy) return false;
//        }

//        var oursGroups = _groupDetails.ToArray();
//        if (other._groupDetails.Count != oursGroups.Length) return false;

//        foreach (var item in oursGroups)
//        {
//            if (!other._groupDetails.TryGetValue(item.Key, out var otherGroup)) return false;
//            if (item.Value != otherGroup) return false;
//        }

//        return true;
//    }

//    public static bool operator ==(GroupCollection? left, GroupCollection? right)
//    {
//        if (left is null) return right is null;
//        return left.Equals(right);
//    }

//    public static bool operator !=(GroupCollection? left, GroupCollection? right) => !(left == right);


//    private Option InternalSetGroup(GroupDetail group, GraphTrxContext? trxContext = null)
//    {
//        group.NotNull().Validate().ThrowOnError();

//        _ = _groupDetails.AddOrUpdate(group.Name,
//            key =>
//            {
//                trxContext?.Recorder?.Add(key, group);
//                return group;
//            },
//            (key, existing) =>
//            {
//                trxContext?.Recorder?.Update(key, existing, group);
//                return group;
//            }
//        );

//        return StatusCode.OK;
//    }

//    private Option InternalRemoveGroup(string groupName, GraphTrxContext? trxContext = null)
//    {
//        if (_groupDetails.TryRemove(groupName, out var existingGroup))
//        {
//            trxContext?.Recorder?.Delete(groupName, existingGroup);
//        }

//        var deletePolicyList = _groupToPolicyIndex.Lookup(groupName);
//        foreach (var policyName in deletePolicyList)
//        {
//            if (_policies.TryRemove(policyName, out var existingPolicy))
//            {
//                trxContext?.Recorder?.Delete(policyName, existingPolicy);
//                _userToGroupIndex.Remove(existingPolicy.PrincipalIdentifier, groupName);
//            }
//        }

//        _groupToPolicyIndex.Remove(groupName);

//        return StatusCode.OK;
//    }

//    private Option InternalSetPolicy(GroupPolicy policy, GraphTrxContext? trxContext = null)
//    {
//        policy.NotNull();
//        policy.NotNull().Validate().ThrowOnError();

//        if (!_groupDetails.TryGetValue(policy.NameIdentifier, out var groupDetail)) return (StatusCode.NotFound, $"Group {policy.NameIdentifier} does not exit");
//        if (!_checkUser(policy.PrincipalIdentifier)) return (StatusCode.NotFound, $"user={policy.PrincipalIdentifier} does not exit");

//        _ = _policies.AddOrUpdate(policy.Key,
//            key =>
//            {
//                trxContext?.Recorder?.Add(key, policy);
//                return policy;
//            },
//            (key, existing) =>
//            {
//                trxContext?.Recorder?.Update(key, existing, policy);
//                return policy;
//            }
//        );

//        _groupToPolicyIndex.Set(groupDetail.Name, policy.Key);
//        _userToGroupIndex.Set(policy.PrincipalIdentifier, groupDetail.Name);

//        return StatusCode.OK;
//    }

//    private Option InternalRemovePolicy(string policyKey, GraphTrxContext? context = null)
//    {
//        policyKey.NotEmpty();

//        if (!_policies.TryRemove(policyKey, out var policy)) return StatusCode.NotFound;

//        _groupToPolicyIndex.RemovePrimaryKey(policy.Key);
//        _userToGroupIndex.Remove(policy.PrincipalIdentifier, policy.NameIdentifier);

//        return StatusCode.OK;
//    }
//}
