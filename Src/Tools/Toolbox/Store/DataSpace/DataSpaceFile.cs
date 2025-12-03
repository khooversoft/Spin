//using Toolbox.Data;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Store;

//public class DataSpaceFile : IKeyStore
//{
//    private readonly IFileStore _fileStore;
//    private readonly DataSpaceOption _option;
//    private readonly LockManager _lockManager;

//    public DataSpaceFile(IFileStore fileStore, LockManager lockManager, DataSpaceOption option)
//    {
//        _fileStore = fileStore.NotNull();
//        _option = option.NotNull().Action(x => x.Validate());
//        _lockManager = lockManager;
//    }

//    public async Task<Option<string>> Append(string path, DataETag data, ScopeContext context)
//    {
//        var file = _lockManager.GetReadWriteAccess(path, context);
//        var result = file.Append(data, context).Result;
//        return result;
//    }

//    public async Task<Option> Delete(string path, ScopeContext context, TrxRecorder? recorder = null)
//    {
//        var result = await _fileStore.File(path).Delete(context).ConfigureAwait(false);
//        return result;
//    }

//    public async Task<Option> Exists(string path, ScopeContext context)
//    {
//        var result = await _fileStore.File(path).Exists(context).ConfigureAwait(false);
//        return result;
//    }

//    public async Task<Option<DataETag>> Get(string path, ScopeContext context)
//    {
//        var file = _lockManager.GetReadWriteAccess(path, context);
//        var result = await file.Get(context);
//        return result;
//    }

//    public async Task<Option<IStorePathDetail>> GetDetails(string path, ScopeContext context)
//    {
//        var result = await _fileStore.File(path).GetDetails(context).ConfigureAwait(false);
//        return result;
//    }

//    public async Task<IReadOnlyList<IStorePathDetail>> Search(string pattern, ScopeContext context)
//    {
//        var result = await _fileStore.Search(pattern, context);
//        return result;
//    }

//    public async Task<Option<string>> Set(string path, DataETag data, ScopeContext context, TrxRecorder? recorder = null)
//    {
//        var file = _lockManager.GetReadWriteAccess(path, context);
//        var result = await file.Set(data, context);
//        return result;
//    }



//    public async Task<Option> AcquireExclusiveLock(string path, bool breakLeaseIfExist, ScopeContext context)
//    {
//        var result = await _lockManager.ProcessLock(path, LockMode.Exclusive, context);
//        return result;
//    }

//    public async Task<Option> AcquireLock(string path, TimeSpan leaseDuration, ScopeContext context)
//    {
//        var result = await _lockManager.ProcessLock(path, LockMode.Exclusive, context);
//        return result;
//    }

//    public async Task<Option> BreakLease(string path, ScopeContext context)
//    {
//        var result = await _lockManager.ReleaseLock(path, context);
//        var makeSure = await _fileStore.File(path).BreakLease(context);
//        return StatusCode.OK;
//    }
//}
