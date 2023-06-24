using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Storage;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Services;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.State;

internal class DatalakeStateConnector : IGrainStorage
{
    private readonly ILogger<DatalakeStateConnector> _logger;
    private readonly ConcurrentDictionary<string, DatalakeState> _stores = new ConcurrentDictionary<string, DatalakeState>(StringComparer.OrdinalIgnoreCase);
    private readonly SiloConfigStore _siloConfigStore;
    private readonly SpinClusterOption _clusterOption;
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);
    private readonly ILoggerFactory _loggerFactory;

    public DatalakeStateConnector(SpinClusterOption clusterOption, SiloConfigStore siloConfigStore, ILoggerFactory loggerFactory)
    {
        _clusterOption = clusterOption.NotNull();
        _siloConfigStore = siloConfigStore.NotNull();
        _loggerFactory = loggerFactory.NotNull();

        _logger = _loggerFactory.CreateLogger<DatalakeStateConnector>();
    }

    public async Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        (string filePath, string schema, ScopeContext context) = VerifyAndGetDetails(grainId, stateName);
        DatalakeState datalakeState = await GetGrainStorageBySchema(schema);

        context.Location().LogInformation("Clearing state for stateName={stateName}, grainId={grainId}", stateName, grainId);
        await datalakeState.ClearStateAsync(filePath, grainState, context);
    }

    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        (string filePath, string schema, ScopeContext context) = VerifyAndGetDetails(grainId, stateName);
        DatalakeState datalakeState = await GetGrainStorageBySchema(schema);

        context.Location().LogInformation("Reading state for stateName={stateName}, grainId={grainId}", stateName, grainId);
        await datalakeState.ReadStateAsync(filePath, grainState, context);
    }

    public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        (string filePath, string schema, ScopeContext context) = VerifyAndGetDetails(grainId, stateName);
        DatalakeState datalakeState = await GetGrainStorageBySchema(schema);

        context.Location().LogInformation("Writing state for stateName={stateName}, grainId={grainId}", stateName, grainId);
        await datalakeState.WriteStateAsync(filePath, grainState, context);
    }

    private (string FilePath, string Schema, ScopeContext Context) VerifyAndGetDetails(GrainId grainId, string stateName)
    {
        var context = new ScopeContext(_logger);

        string grainPath = GetPath(grainId, stateName);

        var objectIdOption = grainPath.ToObjectIdIfValid();
        if (objectIdOption.IsError())
        {
            context.Location().LogError("Invalid ObjectId, id={id}", grainId.ToString());
            throw new ArgumentException($"Invalid ObjectId, id={grainId}");
        }

        ObjectId objectId = objectIdOption.Return();

        string filePath = objectId.Tenant + "/" + objectId.Path + "." + stateName;
        context.Location().LogInformation("GrainId={grainId} to FilePath={filePath}", grainId.ToString(), filePath);

        return (filePath, objectId.Schema, context);
    }

    private static string GetPath(GrainId grainId, string stateName) => grainId.ToString()
        .Split('/', StringSplitOptions.RemoveEmptyEntries)
        .Skip(1)
        .Join("/");

    private async Task<DatalakeState> GetGrainStorageBySchema(string schemaName)
    {
        _logger.LogInformation("Requesting store for schema={schema}", schemaName);

        await _lock.WaitAsync();

        try
        {
            if (_stores.TryGetValue(schemaName, out var storage)) return storage;

            var newStorage = await create(schemaName);
            _stores[schemaName] = newStorage;

            return newStorage;
        }
        finally
        {
            _lock.Release();
        }

        async Task<DatalakeState> create(string name)
        {
            _logger.LogInformation("Creating store for name={name}", name);
            var context = new ScopeContext(_logger);

            Option<SiloConfigOption> siloConfigOption = await _siloConfigStore.Get(context);
            if (siloConfigOption.IsError()) throw new InvalidOperationException("Cannot read Silo configuration option from datalake");

            SchemaOption schemaOption = siloConfigOption.Return().Schemas.FirstOrDefault(x => x.SchemaName == name) ??
                throw new ArgumentException($"Cannot find name={name} in schema options to create data lake storage");

            var option = new DatalakeOption
            {
                AccountName = schemaOption.AccountName,
                ContainerName = schemaOption.ContainerName,
                BasePath = schemaOption.BasePath,
                Credentials = _clusterOption.ClientCredentials,
            };

            IDatalakeStore store = new DatalakeStore(option, _loggerFactory.CreateLogger<DatalakeStore>());

            return new DatalakeState(name, store, _loggerFactory.CreateLogger<DatalakeState>());
        }
    }
}
