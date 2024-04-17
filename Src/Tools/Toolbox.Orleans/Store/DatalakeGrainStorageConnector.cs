using Azure;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Storage;
using Toolbox.Azure;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

internal class DatalakeGrainStorageConnector : IGrainStorage
{
    private readonly IDatalakeManager _datalakeManager;
    private readonly ILogger<DatalakeGrainStorageConnector> _logger;

    public DatalakeGrainStorageConnector(IDatalakeManager datalakeManager, ILogger<DatalakeGrainStorageConnector> logger)
    {
        _datalakeManager = datalakeManager.NotNull();
        _logger = logger.NotNull();
    }

    public async Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var context = new ScopeContext(_logger);

        (string filePath, IDatalakeStore store) = GetStoreAndPath(grainId, stateName);
        context.LogInformation("Clearing state for filePath={filePath}", filePath);

        var result = await store.Delete(filePath, context);
        if (result.IsError()) return;

        ResetState(grainState);
    }

    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var context = new ScopeContext(_logger);
        context = context.With(_logger);

        (string filePath, IDatalakeStore store) = GetStoreAndPath(grainId, stateName);
        context.LogInformation("Reading state for filePath={filePath}", filePath);

        var result = await store.Read(filePath, context);
        if (result.IsError())
        {
            result.LogStatus(context, "Reading file from datalake");
            ResetState(grainState);
            return;
        }

        try
        {
            DataETag dataETag = result.Return();

            grainState.State = typeof(T) switch
            {
                Type v when v == typeof(DataETag) => dataETag.Cast<T>(),
                _ => dataETag.Data.BytesToString().ToObject<T>().NotNull(),
            };

            grainState.RecordExists = true;
            grainState.ETag = result.Return().ETag.NotEmpty().ToString();
            context.LogInformation("File has been read, filePath={filePath}, ETag={etag}", filePath, grainState.ETag);
            return;
        }
        catch (Exception ex)
        {
            context.Location().LogError("Failed to parse file path={path}, ex={ex}", filePath, ex.ToString());
            ResetState(grainState);
            return;
        }
    }

    public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var context = new ScopeContext(_logger);

        (string filePath, IDatalakeStore store) = GetStoreAndPath(grainId, stateName);
        context.Location().LogInformation("Writing state for filePath={filePath}", filePath);

        DataETag dataEtag = grainState.State switch
        {
            DataETag v => new DataETag(v.Data, grainState.ETag),

            var v => v
                .ToJsonSafe(context.Location())
                .ToBytes()
                .Func(x => new DataETag(x, grainState.ETag)),
        };

        var result = await store.Write(filePath, dataEtag, true, context);
        if (result.IsError())
        {
            context.Location().LogError("Failed to write state file, filePath={filePath}", filePath);
            ResetState(grainState);
            return;
        }

        grainState.RecordExists = true;
        grainState.ETag = result.Return().ToString();
    }

    private void ResetState<T>(IGrainState<T> grainState)
    {
        grainState.State = default!;
        grainState.RecordExists = false;
        grainState.ETag = null;
    }

    private (string path, IDatalakeStore store) GetStoreAndPath(GrainId grainId, string extension)
    {
        string filePath = GetPath(grainId, extension);
        IDatalakeStore store = _datalakeManager.MapToStore(filePath).ThrowOnError($"FilePath={filePath} failed to map to store").Return();

        return (filePath, store);
    }

    private static string GetPath(GrainId grainId, string extension) => grainId.ToString()
        .Split('/', StringSplitOptions.RemoveEmptyEntries)
        .Skip(1)
        .Join("/")
        .Func(x => extension.IsEmpty() ? x : PathTool.SetExtension(x, extension));
}
