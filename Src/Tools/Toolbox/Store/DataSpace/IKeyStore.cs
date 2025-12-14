using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Types;

namespace Toolbox.Store;

public interface IKeyStore
{
    Task<Option<string>> Add(string key, DataETag data, ScopeContext context, TrxRecorder? recorder = null);
    Task<Option<string>> Append(string key, DataETag data, ScopeContext context, string? leaseId = null);
    Task<Option> Delete(string key, ScopeContext context, TrxRecorder? recorder = null, string? leaseId = null);
    Task<Option<DataETag>> Get(string key, ScopeContext context);
    Task<Option<string>> Set(string key, DataETag data, ScopeContext context, TrxRecorder? recorder = null, string? leaseId = null);

    Task<Option> DeleteFolder(string key, ScopeContext context);
    Task<Option> Exists(string key, ScopeContext context);
    Task<Option<StorePathDetail>> GetDetails(string key, ScopeContext context);
    Task<IReadOnlyList<StorePathDetail>> Search(string pattern, ScopeContext context);

    Task<Option> BreakLease(string key, ScopeContext context);
    Task<Option<string>> AcquireExclusiveLock(string key, bool breakLeaseIfExist, ScopeContext context);
    Task<Option<string>> AcquireLease(string key, TimeSpan leaseDuration, ScopeContext context);
    Task<Option> Release(string leaseId, ScopeContext context);
}

public interface IKeyReadWriteStore
{
    Task<Option<string>> Append(DataETag data, ScopeContext context);
    Task<Option> Delete(ScopeContext context);
    Task<Option<DataETag>> Get(ScopeContext context);
    Task<Option<string>> Set(DataETag data, ScopeContext context);
    Task<Option> Release(ScopeContext context);
}
