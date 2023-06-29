﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Storage;
using Toolbox.Azure.DataLake;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Extensions;
using Azure;

namespace SpinCluster.sdk.State;

public class DatalakeStateHandler
{
    private readonly string _storageName;
    private readonly IDatalakeStore _datalakeStore;
    private readonly ILogger<DatalakeStateHandler> _logger;

    public DatalakeStateHandler(string storageName, IDatalakeStore datalakeStore, ILogger<DatalakeStateHandler> logger)
    {
        _storageName = storageName.NotEmpty();
        _datalakeStore = datalakeStore.NotNull();
        _logger = logger.NotNull();
    }

    public async Task ClearStateAsync<T>(string filePath, IGrainState<T> grainState, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Clearing state for filePath={filePath}", filePath);

        var result = await _datalakeStore.Delete(filePath, context);
        if (result.IsError())
        {
            context.Location().LogError("Failed to delete state file on datalake, storageName={storageName}, filePath={filePath}",
                _storageName, filePath);
        }

        ResetState(grainState);
    }

    public async Task ReadStateAsync<T>(string filePath, IGrainState<T> grainState, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Reading state for filePath={filePath}", filePath);

        var result = await _datalakeStore.Read(filePath, context);
        if (result.IsError())
        {
            context.Location().LogWarning("Failed to read state file, storageName={storageName}, filePath={filePath}",
                _storageName, filePath);

            ResetState(grainState);
            return;
        }

        try
        {
            grainState.State = result.Return().Data
                .BytesToString()
                .ToObject<T>()
                .NotNull();
        }
        catch (Exception ex)
        {
            context.Location().LogError("Failed to parse file path={path}, ex={ex}", filePath, ex.ToString());
            ResetState(grainState);
            return;
        }

        grainState.RecordExists = true;
        grainState.ETag = result.Return().ETag.ToString();
    }

    public async Task WriteStateAsync<T>(string filePath, IGrainState<T> grainState, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Writing state for filePath={filePath}", filePath);

        byte[] data = grainState.State
            .ToJsonSafe(context.Location())
            .ToBytes();

        ETag etag = new ETag(grainState.ETag);
        var dataEtag = new DataETag(data, etag);

        var result = await _datalakeStore.Write(filePath, dataEtag, true, context);
        if (result.IsError())
        {
            context.Location().LogError("Failed to write state file, storageName={storageName}, filePath={filePath}",
                _storageName, filePath);

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

    //public void Participate(ISiloLifecycle lifecycle) =>
    //lifecycle.Subscribe(
    //    observerName: OptionFormattingUtilities.Name<DatalakeStorage>(_storageName),
    //    stage: ServiceLifecycleStage.ApplicationServices,
    //    onStart: (ct) =>
    //    {
    //        Console.WriteLine("Participating");
    //        return Task.CompletedTask;
    //    });
}