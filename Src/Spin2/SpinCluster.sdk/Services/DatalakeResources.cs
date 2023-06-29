using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Configuration;
using SpinCluster.sdk.Application;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Services;

public class DatalakeResources
{
    private readonly SpinClusterOption _clusterOption;
    private readonly ILogger<DatalakeResources> _logger;
    private readonly SiloConfigStore _siloConfigStore;
    private readonly ConcurrentDictionary<string, IDatalakeStore> _datalakeResources = new ConcurrentDictionary<string, IDatalakeStore>(StringComparer.OrdinalIgnoreCase);
    private readonly ILoggerFactory _loggerFactory;

    public DatalakeResources(SpinClusterOption clusterOption, SiloConfigStore siloConfigStore, ILoggerFactory loggerFactory)
    {
        _clusterOption = clusterOption.NotNull();
        _siloConfigStore = siloConfigStore.NotNull();
        _loggerFactory = loggerFactory.NotNull();

        _logger = loggerFactory.NotNull().CreateLogger<DatalakeResources>();
    }

    public async Task<StatusCode> Startup(ScopeContext context)
    {
        Option<SiloConfigOption> siloConfigOption = await _siloConfigStore.Get(context);
        if (siloConfigOption.IsError())
        {
            const string msg = "Failed to get silo configuration";
            context.Location().LogCritical(msg);
            throw new InvalidOperationException(msg);
        }

        var query = new QueryParameter { Count = 1 };
        var list = new List<Task<bool>>();

        foreach (var schemaOption in siloConfigOption.Return().Schemas)
        {
            var option = new DatalakeOption
            {
                AccountName = schemaOption.AccountName,
                ContainerName = schemaOption.ContainerName,
                BasePath = schemaOption.BasePath,
                Credentials = _clusterOption.ClientCredentials,
            };

            IDatalakeStore store = new DatalakeStore(option, _loggerFactory.CreateLogger<DatalakeStore>());
            context.Location().LogInformation("Setting up schemaName={schemaName}, option={option}", schemaOption.SchemaName, option);

            _datalakeResources[schemaOption.SchemaName] = store;

            list.Add(verifyConnection(store, schemaOption));
        }

        // Verify that the Silo has access to all datalake resources
        var results = await Task.WhenAll(list);
        return results.All(x => x) ? StatusCode.OK : StatusCode.BadRequest;


        async Task<bool> verifyConnection(IDatalakeStore store, SchemaOption schemaOption)
        {
            bool exist = await store.TestConnection(context);
            if (!exist)
            {
                context.Location().LogCritical("Failed to connect to datalake store schemaOption={schemaOption}, credentials={credentials}",
                    schemaOption, _clusterOption.ClientCredentials);

                return false;
            }

            context.Location().LogInformation("Connected to datalake store schemaOption={schemaOption}", schemaOption);
            return true;
        }
    }

    public Option<IDatalakeStore> GetStore(string schemaName) => _datalakeResources.TryGetValue(schemaName, out var store) switch
    {
        true => store.ToOption(),
        false => new Option<IDatalakeStore>(StatusCode.NotFound),
    };

    public async Task<Option<IReadOnlyList<DatalakePathItem>>> Search(string schemaName, QueryParameter queryParameter, ScopeContext context)
    {
        var store = GetStore(schemaName);
        if (store.IsError())
        {
            context.Location().LogError("Schema not found, schemaName={schemaName}", schemaName);
            return store.ToOption<IReadOnlyList<DatalakePathItem>>();
        }

        var result = await store.Return().Search(queryParameter, context);
        if (result.IsError())
        {
            context.Location().LogCritical("Error in searching schemaName={schemaName}, queryParameter={queryParameter}", schemaName, queryParameter);
            return result;
        }

        return result;
    }
}
