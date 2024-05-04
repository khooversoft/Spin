using Microsoft.Extensions.DependencyInjection;
using Orleans.Storage;
using Toolbox.Azure;
using Toolbox.Graph;

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

    public static IServiceCollection AddDirectoryClient(this IServiceCollection services)
    {
        //services.AddSingleton<IGraphStore, DirectoryStoreActorConnector>();
        //services.AddSingleton<IGraphCommand, DirectoryActorConnector>();
        //services.AddSingleton<IGraphEntity, GraphEntity>();

        return services;
    }
}
