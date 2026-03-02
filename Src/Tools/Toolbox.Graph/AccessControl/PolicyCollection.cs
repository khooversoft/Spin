//using System;
//using System.Collections;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph;

//public class PolicyCollection : IEnumerable<GroupPolicy>
//{
//    private readonly ConcurrentDictionary<string, GroupPolicy> _policies = new(StringComparer.OrdinalIgnoreCase);
//    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);
//    private readonly Func<string, bool> _checkUser;

//    public PolicyCollection(IEnumerable<GroupPolicy> groupPolicies, Func<string, bool> checkUser)
//    {
//        var policies = groupPolicies.NotNull().ToArray();
//        foreach (var item in policies) _policies.TryAdd(item.Key, item).BeTrue();
//        _checkUser = checkUser;
//    }

//    public void Clear()
//    {
//        _lock.EnterWriteLock();
//        try
//        {
//            _policies.Clear();
//        }
//        finally { _lock.ExitWriteLock(); }
//    }

//    public Option Set(GroupPolicy policy, GraphTrxContext? trxContext = null)
//    {
//        policy.NotNull();
//        policy.NotNull().Validate().ThrowOnError();

//        _lock.EnterWriteLock();
//        try
//        {
//            if (!_checkUser(policy.PrincipalIdentifier)) return (StatusCode.NotFound, $"user={policy.PrincipalIdentifier} does not exit");

//            _ = _policies.AddOrUpdate(policy.Key,
//                key =>
//                {
//                    trxContext?.Recorder?.Add(key, policy);
//                    return policy;
//                },
//                (key, existing) =>
//                {
//                    trxContext?.Recorder?.Update(key, existing, policy);
//                    return policy;
//                }
//            );

//            return StatusCode.OK;
//        }
//        finally { _lock.ExitWriteLock(); }
//    }

//    public Option Remove(string policyKey, GraphTrxContext? trxContext = null)
//    {
//        policyKey.NotEmpty();

//        _lock.EnterWriteLock();
//        try
//        {
//            if (_policies.TryRemove(policyKey, out var existing))
//            {
//                trxContext?.Recorder?.Delete(existing.Key, existing);
//            }
//        }
//        finally { _lock.ExitWriteLock(); }

//        return StatusCode.OK;
//    }

//    public bool TryGetPolicy(string policyKey, out GroupPolicy? group)
//    {
//        _lock.EnterReadLock();
//        try
//        { 
//            return _policies.TryGetValue(policyKey.NotEmpty(), out group);
//        }
//        finally { _lock.ExitReadLock(); }
//    }

//    public IEnumerator<GroupPolicy> GetEnumerator()
//    {
//        List<GroupPolicy> snapshot;

//        _lock.EnterReadLock();
//        try { snapshot = _policies.Values.ToList(); }
//        finally { _lock.ExitReadLock(); }

//        return snapshot.GetEnumerator();
//    }

//    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
//}
