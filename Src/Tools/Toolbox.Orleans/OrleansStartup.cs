//using Microsoft.Extensions.DependencyInjection;
//using Orleans.Storage;
//using Toolbox.Azure;
//using Toolbox.Store;

//namespace Toolbox.Orleans;

//public static class OrleansStartup
//{
//    public static IServiceCollection AddGrainFileStore(this IServiceCollection services)
//    {
//        services.AddKeyedSingleton<IGrainStorage, GrainStorageFileStoreConnector>(OrleansConstants.StorageProviderName);
//        services.AddSingleton<IFileStore, GraphFileStoreActorConnector>();

//        //services.AddSingleton<IdentityActorConnector>();
//        return services;
//    }
//}
