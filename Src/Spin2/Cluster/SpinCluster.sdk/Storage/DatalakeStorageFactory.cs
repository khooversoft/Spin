using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Storage;
using SpinCluster.sdk.Application;
using Toolbox.Azure.DataLake;
using Toolbox.Tools;

namespace SpinCluster.sdk.Storage;

public class DatalakeStorageFactory
{
    private readonly ILogger<DatalakeStorageFactory> _logger;
    private readonly ConcurrentDictionary<string, DatalakeStorage> _stores = new ConcurrentDictionary<string, DatalakeStorage>(StringComparer.OrdinalIgnoreCase);
    private readonly SpinClusterOption _option;

    public DatalakeStorageFactory(SpinClusterOption option, ILogger<DatalakeStorageFactory> logger)
    {
        _option = option.NotNull();
        _logger = logger.NotNull();
    }

    public IGrainStorage CreateStorage(IServiceProvider service, string name)
    {
        _logger.LogInformation("Requesting store for name={name}", name);

        return _stores.GetOrAdd(name, create);


        DatalakeStorage create(string name)
        {
            _logger.LogInformation("Creating store for name={name}", name);

            SchemaOption schemaOption = _option.Schemas.FirstOrDefault(x => x.SchemaName == name) ??
                throw new ArgumentException($"Cannot find name={name} in schema options to create data lake storage");

            ILoggerFactory loggerFactory = service.GetRequiredService<ILoggerFactory>();


            var option = new DatalakeOption
            {
                AccountName = schemaOption.AccountName,
                ContainerName = schemaOption.ContainerName,
                BasePath = schemaOption.BasePath,
                Credentials = _option.ClientCredentials,
            };

            IDatalakeStore store = new DatalakeStore(option, loggerFactory.CreateLogger<DatalakeStore>());

            return new DatalakeStorage(name, store, loggerFactory.CreateLogger<DatalakeStorage>());
        }
    }
}
