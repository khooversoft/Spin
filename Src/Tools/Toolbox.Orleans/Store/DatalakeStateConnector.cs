using Azure;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Storage;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

internal class DatalakeStateConnector : IGrainStorage
{
    private readonly IDatalakeStore _datalakeStore;
    private readonly ILogger<DatalakeStateConnector> _logger;

    public DatalakeStateConnector(IDatalakeStore datalakeStore, ILogger<DatalakeStateConnector> logger)
    {
        _datalakeStore = datalakeStore;
        _logger = logger;
    }

    public async Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var context = new ScopeContext(_logger);

        string filePath = GetPath(grainId);
        context.LogInformation("Clearing state for filePath={filePath}", filePath);

        var result = await _datalakeStore.Delete(filePath, context);
        if (result.IsError()) return;

        ResetState(grainState);
    }

    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var context = new ScopeContext(_logger);
        context = context.With(_logger);

        string filePath = GetPath(grainId);
        context.LogInformation("Reading state for filePath={filePath}", filePath);

        var result = await _datalakeStore.Read(filePath, context);
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
            grainState.ETag = result.Return().ETag.ToString();
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

        string filePath = GetPath(grainId);
        context.Location().LogInformation("Writing state for filePath={filePath}", filePath);

        ETag etag = new ETag(grainState.ETag);

        DataETag dataEtag = grainState.State switch
        {
            DataETag v => new DataETag(v.Data, etag),

            var v => v
                .ToJsonSafe(context.Location())
                .ToBytes()
                .Func(x => new DataETag(x, etag)),
        };

        var result = await _datalakeStore.Write(filePath, dataEtag, true, context);
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

    private static string GetPath(GrainId grainId) => grainId.ToString()
        .Split('/', StringSplitOptions.RemoveEmptyEntries)
        .Skip(1)
        .Join("/");
}
