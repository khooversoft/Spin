using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal class FileStoreTraceShim : IGraphFileStore
{
    private readonly IFileStore _filestore;
    private readonly IChangeTrace _changeTrace;

    public FileStoreTraceShim(IFileStore filestore, IChangeTrace changeTrace)
    {
        _filestore = filestore.NotNull();
        _changeTrace = changeTrace.NotNull();
    }

    public async Task<Option<string>> Add(string path, DataETag data, ScopeContext context)
    {
        var result = await _filestore.Add(path, data, context);
        if (result.IsOk())
        {
            var trx = new ChangeTrx(ChangeTrxType.FileAdd, path, data);
            _changeTrace.Log(trx);
        }

        return result;
    }

    public async Task<Option> Delete(string path, ScopeContext context)
    {
        var result = await _filestore.Delete(path, context);
        if (result.IsOk())
        {
            var trx = new ChangeTrx(ChangeTrxType.FileDelete, path, null);
            _changeTrace.Log(trx);
        }

        return result;
    }

    public Task<Option> Exist(string path, ScopeContext context) => _filestore.Exist(path, context);
    public Task<Option<DataETag>> Get(string path, ScopeContext context) => _filestore.Get(path, context);
    public Task<IReadOnlyList<string>> Search(string pattern, ScopeContext context) => _filestore.Search(pattern, context);

    public async Task<Option<string>> Set(string path, DataETag data, ScopeContext context)
    {
        var result = await _filestore.Set(path, data, context);
        if (result.IsOk())
        {
            var trx = new ChangeTrx(ChangeTrxType.FileDelete, path, data);
            _changeTrace.Log(trx);
        }

        return result;
    }
}
