using Microsoft.Extensions.DependencyInjection;
using Orleans.Storage;
using Toolbox.Azure;
using Toolbox.Graph;

namespace Toolbox.Orleans;

public static class OrleansStartup
{
    public static IServiceCollection AddGrainFileStore(this IServiceCollection services)
    {
        services.AddKeyedSingleton<IGrainStorage, GrainStorageFileStoreConnector>(OrleansConstants.StorageProviderName);
        services.AddSingleton<IGraphFileStore, GraphFileStoreActorConnector>();

        services.AddSingleton<IdentityActorConnector>();
        return services;
    }
}
