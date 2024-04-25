using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

public interface IFileStoreSearchActor : IGrainWithStringKey
{
    Task<IReadOnlyList<string>> Search(string pattern, ScopeContext context);
}

[StatelessWorker]
public class FileStoreSearchActor : Grain, IFileStoreSearchActor
{
    private readonly ILogger<FileStoreSearchActor> _logger;
    private readonly IStoreCollection _storeCollection;

    public FileStoreSearchActor(IStoreCollection storeCollection, ILogger<FileStoreSearchActor> logger)
    {
        _storeCollection = storeCollection.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<IReadOnlyList<string>> Search(string pattern, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogInformation("Searching file store pattern={pattern}", pattern);

        (string alias, string filePath) = _storeCollection.GetAliasAndPath(pattern);
        IFileStore fileStore = _storeCollection.Get(alias);

        IReadOnlyList<string> result = await fileStore.Search(filePath, context);
        context.LogInformation("Searched file store pattern={pattern}, count={count}", pattern, result.Count);
        return result;
    }
}