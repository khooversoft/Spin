using Microsoft.Extensions.DependencyInjection;
using Orleans.Storage;
using Toolbox.Azure;

namespace Toolbox.Orleans;

public static class OrleansStartup
{
    //public static ISiloBuilder AddDatalakeGrainStorage(this ISiloBuilder builder) =>
    //    builder.ConfigureServices(services =>
    //    {
    //        services.AddKeyedSingleton<IGrainStorage, DatalakeGrainStorageConnector>(OrleansConstants.StorageProviderName);
    //    });

    public static IServiceCollection AddGrainFileStorage(this IServiceCollection services)
    {
        services.AddKeyedSingleton<IGrainStorage, GrainStorageFileStoreConnector>(OrleansConstants.StorageProviderName);
        return services;
    }
}
