using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Storage;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public class GrainStorageFileStoreConnector : IGrainStorage
{
    private readonly IStoreCollection _storeCollection;
    private readonly ILogger<GrainStorageFileStoreConnector> _logger;

    public GrainStorageFileStoreConnector(IStoreCollection storeCollection, ILogger<GrainStorageFileStoreConnector> logger)
    {
        _storeCollection = storeCollection.NotNull();
        _logger = logger.NotNull();
    }

    public async Task ClearStateAsync<T>(string extension, GrainId grainId, IGrainState<T> grainState)
    {
        var context = new ScopeContext(_logger);

        (string filePath, IFileStore store) = GetStoreAndPath(grainId, extension);
        context.LogInformation("Clearing state for filePath={filePath}", filePath);

        var result = await store.Delete(filePath, context);
        if (result.IsError()) return;

        ResetState(grainState);
    }

    public async Task ReadStateAsync<T>(string extension, GrainId grainId, IGrainState<T> grainState)
    {
        var context = new ScopeContext(_logger);
        context = context.With(_logger);

        (string filePath, IFileStore store) = GetStoreAndPath(grainId, extension);
        context.LogInformation("Reading state for filePath={filePath}", filePath);

        var result = await store.Get<T>(filePath, context);
        if (result.IsError())
        {
            result.LogStatus(context, "Reading file from datalake");
            ResetState(grainState);
            return;
        }

        grainState.State = result.Return().Value;
        grainState.RecordExists = true;
        grainState.ETag = result.Return().ETag;
        context.LogInformation("File has been read, filePath={filePath}, ETag={etag}", filePath, grainState.ETag);
    }

    public async Task WriteStateAsync<T>(string extension, GrainId grainId, IGrainState<T> grainState)
    {
        var context = new ScopeContext(_logger);

        (string filePath, IFileStore store) = GetStoreAndPath(grainId, extension);
        context.Location().LogInformation("Writing state for filePath={filePath}", filePath);

        DataETag dataEtag = grainState.State switch
        {
            DataETag v => new DataETag(v.Data, grainState.ETag),

            var v => v
                .ToJsonSafe(context.Location())
                .ToBytes()
                .Func(x => new DataETag(x, grainState.ETag)),
        };

        var result = await store.Set(filePath, dataEtag, context);
        if (result.IsError())
        {
            context.Location().LogError("Failed to write state file, filePath={filePath}", filePath);
            ResetState(grainState);
            return;
        }

        grainState.RecordExists = true;
        grainState.ETag = result.Return().ToString();
    }

    private (string filePath, IFileStore store) GetStoreAndPath(GrainId grainId, string extension)
    {
        string path = grainId.ToString()
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .Join('/');

        (string alias, string filePath) = _storeCollection.GetAliasAndPath(path, extension);
        IFileStore store = _storeCollection.Get(alias);

        return (filePath, store);
    }

    private void ResetState<T>(IGrainState<T> grainState)
    {
        grainState.State = default!;
        grainState.RecordExists = false;
        grainState.ETag = null;
    }
}
