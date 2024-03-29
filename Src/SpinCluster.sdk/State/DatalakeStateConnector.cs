﻿using System.Collections.Concurrent;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Storage;
using SpinCluster.sdk.Services;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.State;

internal class DatalakeStateConnector : IGrainStorage
{
    private readonly ILogger<DatalakeStateConnector> _logger;
    private readonly ConcurrentDictionary<string, DatalakeStateHandler> _stores = new ConcurrentDictionary<string, DatalakeStateHandler>(StringComparer.OrdinalIgnoreCase);
    private readonly DatalakeSchemaResources _datalakeResources;
    private readonly ILoggerFactory _loggerFactory;
    private readonly DatalakeResourceIdMap _map;
    private readonly TelemetryClient _telemetryClient;

    public DatalakeStateConnector(DatalakeSchemaResources datalakeResources, DatalakeResourceIdMap map, TelemetryClient telemetryClient, ILoggerFactory loggerFactory)
    {
        _datalakeResources = datalakeResources.NotNull();
        _loggerFactory = loggerFactory.NotNull();
        _map = map.NotNull();

        _logger = _loggerFactory.CreateLogger<DatalakeStateConnector>();
        _telemetryClient = telemetryClient;
    }

    public async Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        (string filePath, string schema, ScopeContext context) = VerifyAndGetDetails(grainId, stateName);
        DatalakeStateHandler datalakeState = GetGrainStorageBySchema(schema, context);

        context.Location().LogInformation("Clearing state for stateName={stateName}, grainId={grainId}", stateName, grainId);
        await datalakeState.ClearStateAsync(filePath, grainState, context);
    }

    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        (string filePath, string schema, ScopeContext context) = VerifyAndGetDetails(grainId, stateName);
        DatalakeStateHandler datalakeState = GetGrainStorageBySchema(schema, context);

        context.Location().LogInformation("Reading state for stateName={stateName}, grainId={grainId}", stateName, grainId);
        await datalakeState.ReadStateAsync(filePath, grainState, context);
    }

    public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        (string filePath, string schema, ScopeContext context) = VerifyAndGetDetails(grainId, stateName);
        DatalakeStateHandler datalakeState = GetGrainStorageBySchema(schema, context);

        context.Location().LogInformation("Writing state for stateName={stateName}, grainId={grainId}", stateName, grainId);
        await datalakeState.WriteStateAsync(filePath, grainState, context);
    }

    private (string FilePath, string Schema, ScopeContext Context) VerifyAndGetDetails(GrainId grainId, string stateName)
    {
        var context = new ScopeContext(_logger);

        string grainPath = GetPath(grainId);

        ResourceId resourceId = ResourceId.Create(grainPath).ThrowOnError().Return();
        Option<DatalakeResourceIdMap.Map> mapOption = _map.MapResource(resourceId, context);
        if (mapOption.IsError())
        {
            context.Location().LogCritical("Failed to map schemato storage resource for resourceId={resourceId}", resourceId);
            throw new InvalidOperationException($"Failed to map schemato storage resource for resourceId={resourceId}");
        }

        context.Location().LogInformation("GrainId={grainId} to resourceId={resourceId}, map={map}", grainId.ToString(), resourceId, mapOption);

        DatalakeResourceIdMap.Map map = mapOption.Return();
        return (map.FilePath, map.Schema, context);
    }

    private static string GetPath(GrainId grainId) => grainId.ToString()
        .Split('/', StringSplitOptions.RemoveEmptyEntries)
        .Skip(1)
        .Join("/");

    private DatalakeStateHandler GetGrainStorageBySchema(string schemaName, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Requesting store for schema={schema}", schemaName);

        return _stores.GetOrAdd(schemaName, x =>
        {
            Option<IDatalakeStore> store = _datalakeResources.GetStore(x);

            if (store.IsError())
            {
                context.Location().LogCritical("Failed to get datalake connection to schemaName={schemaName}", schemaName);
                throw new ArgumentException($"Failed to get datalake connection to schemaName={schemaName}", schemaName);
            }

            return new DatalakeStateHandler(schemaName, store.Return(), _telemetryClient, _loggerFactory.CreateLogger<DatalakeStateHandler>());
        });
    }
}
