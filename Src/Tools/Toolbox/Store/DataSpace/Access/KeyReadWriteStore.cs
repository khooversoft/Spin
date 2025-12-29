//using Toolbox.Data;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Store;

//public class KeyReadWriteStore : IKeyReadWriteStore
//{
//    private readonly IKeyStore _keyStore;
//    private readonly string _key;
//    private readonly string? _leaseId;
//    private readonly TrxRecorder? _recorder;

//    public KeyReadWriteStore(IKeyStore keyStore, string key, string? leaseId = null, TrxRecorder? recorder = null)
//    {
//        _keyStore = keyStore.NotNull();
//        _key = key.NotEmpty();
//        _leaseId = leaseId;
//        _recorder = recorder;
//    }

//    public Task<Option<string>> Append(DataETag data, ScopeContext context) => _keyStore.Append(_key, data, context, _leaseId);
//    public Task<Option> Delete(ScopeContext context) => _keyStore.Delete(_key, context, _recorder, _leaseId);
//    public Task<Option<DataETag>> Get(ScopeContext context) => _keyStore.Get(_key, context);

//    public Task<Option> Release(ScopeContext context)
//    {
//        if (_leaseId == null) return new Option(StatusCode.OK).ToTaskResult();
//        return _keyStore.Release(_leaseId, context);
//    }

//    public Task<Option<string>> Set(DataETag data, ScopeContext context) => _keyStore.Set(_key, data, context, _recorder, _leaseId);
//}
