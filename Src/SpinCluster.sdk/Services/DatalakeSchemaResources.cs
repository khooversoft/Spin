using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Azure;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Services;

public class DatalakeSchemaResources
{
    private const string _defaultSchema = "$default";
    private readonly SpinClusterOption _clusterOption;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatalakeSchemaResources> _logger;
    private readonly SiloConfigStore _siloConfigStore;
    private readonly ConcurrentDictionary<string, IDatalakeStore> _datalakeResources = new ConcurrentDictionary<string, IDatalakeStore>(StringComparer.OrdinalIgnoreCase);
    private readonly string _instanceId = Guid.NewGuid().ToString();

    public DatalakeSchemaResources(SpinClusterOption clusterOption, SiloConfigStore siloConfigStore, IServiceProvider serviceProvider, ILogger<DatalakeSchemaResources> logger)
    {
        Debug.WriteLine($"Constructed: {nameof(DatalakeSchemaResources)}, _instanceId={_instanceId}");
        _clusterOption = clusterOption.NotNull();
        _siloConfigStore = siloConfigStore.NotNull();
        _serviceProvider = serviceProvider.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Startup(ScopeContext context)
    {
        Debug.WriteLine($"Here: {nameof(DatalakeSchemaResources)}, _instanceId={_instanceId}");

        Option<SiloConfigOption> siloConfigOption = await _siloConfigStore.Get(context);
        if (siloConfigOption.IsError())
        {
            const string msg = "Failed to get silo configuration";
            context.Location().LogCritical(msg);
            throw new InvalidOperationException(msg);
        }

        var query = new QueryParameter { Count = 1 };
        var list = new List<Task<bool>>();

        IReadOnlyList<SchemaOption> schemas = siloConfigOption.Return().Schemas;
        schemas.Assert(x => x.Count > 0, _ => "No schemas are available");

        foreach (var schemaOption in siloConfigOption.Return().Schemas)
        {
            var option = new DatalakeOption
            {
                Account = _siloConfigStore.DatalakeLocation.Account,
                Container = schemaOption.ContainerName,
                BasePath = schemaOption.BasePath,
                Credentials = _clusterOption.Credentials,
            };

            IDatalakeStore store = ActivatorUtilities.CreateInstance<DatalakeStore>(_serviceProvider, option);
            context.Location().LogInformation("Setting up schemaName={schemaName}, option={option}", schemaOption.SchemaName, option);

            _datalakeResources[schemaOption.SchemaName] = store;

            list.Add(VerifyConnection(store, schemaOption, context));
        }

        Debug.WriteLine($"Here - completed: {nameof(DatalakeSchemaResources)}, _instanceId={_instanceId}, count={schemas.Count}");

        // Verify that the Silo has access to all datalake resources
        var results = await Task.WhenAll(list);
        return results.All(x => x) ? StatusCode.OK : StatusCode.BadRequest;
    }

    public Option<IDatalakeStore> GetStore(string schemaName) => _datalakeResources.TryGetValue(schemaName, out var store) switch
    {
        true => store.ToOption(),
        false => _datalakeResources.TryGetValue(_defaultSchema, out var store2) switch
        {
            true => store2.ToOption(),
            false => StatusCode.NotFound,
        },
    };

    private async Task<bool> VerifyConnection(IDatalakeStore store, SchemaOption schemaOption, ScopeContext context)
    {
        bool exist = await store.TestConnection(context);
        if (!exist)
        {
            context.Location().LogCritical("Failed to connect to datalake store schemaOption={schemaOption}, credentials={credentials}",
                schemaOption, _clusterOption.Credentials);

            return false;
        }

        context.Location().LogInformation("Connected to datalake store schemaOption={schemaOption}", schemaOption);
        return true;
    }
}
