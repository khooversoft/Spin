using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Data;
using Toolbox.Types;

namespace Toolbox.Store;

public interface IKeyStore
{
    Task<Option<string>> Add(string path, DataETag data, ScopeContext context);
    Task<Option<string>> Append(string path, DataETag data, ScopeContext context);
    Task<Option> Delete(string path, ScopeContext context, TrxRecorder? recorder = null);
    Task<Option> Exists(string path, ScopeContext context);
    Task<Option<DataETag>> Get(string path, ScopeContext context);
    Task<Option<IStorePathDetail>> GetDetails(string path, ScopeContext context);
    Task<IReadOnlyList<IStorePathDetail>> Search(string pattern, ScopeContext context);
    Task<Option<string>> Set(string path, DataETag data, ScopeContext context, TrxRecorder? recorder = null);
    Task<Option> BreakLease(string path, ScopeContext context);
    Task<Option> AcquireExclusiveLock(string path, bool breakLeaseIfExist, ScopeContext context);
    Task<Option<IFileLeasedAccess>> AcquireLease(string path, TimeSpan leaseDuration, ScopeContext context);
}
