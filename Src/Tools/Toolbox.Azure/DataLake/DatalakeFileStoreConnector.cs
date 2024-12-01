using System.Collections.Immutable;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public class DatalakeFileStoreConnector : IFileStore
{
    private readonly IDatalakeStore _datalakeStore;
    public DatalakeFileStoreConnector(IDatalakeStore datalakeStore) => _datalakeStore = datalakeStore.NotNull();

    public async Task<Option<string>> Add(string path, DataETag data, ScopeContext context)
    {
        var result = await _datalakeStore.Write(path, data, false, context).ConfigureAwait(false);
        if (result.IsError()) return result.ToOptionStatus<string>();

        return result.Return().ToString();
    }

    public async Task<Option> Append(string path, DataETag data, ScopeContext context)
    {
        var result = await _datalakeStore.Append(path, data, context).ConfigureAwait(false);
        return result;
    }

    public async Task<Option> Delete(string path, ScopeContext context)
    {
        var result = await _datalakeStore.Delete(path, context).ConfigureAwait(false);
        return result;
    }

    public async Task<Option> Exist(string path, ScopeContext context) => await _datalakeStore.Exist(path, context).ConfigureAwait(false);

    public Task<Option<DataETag>> Get(string path, ScopeContext context) => _datalakeStore.Read(path, context);

    public async Task<IReadOnlyList<string>> Search(string pattern, ScopeContext context)
    {
        var query = QueryParameter.Parse(pattern);
        var result = await _datalakeStore.Search(query, context).ConfigureAwait(false);
        if (result.IsError()) return ImmutableArray<string>.Empty;

        var list = result.Return().Items.Select(x => x.Name).ToImmutableArray();
        return list;
    }

    public async Task<Option<string>> Set(string path, DataETag data, ScopeContext context)
    {
        var result = await _datalakeStore.Write(path, data, true, context).ConfigureAwait(false);
        if (result.IsError()) return result.ToOptionStatus<string>();

        return result.Return().ToString();
    }
}
