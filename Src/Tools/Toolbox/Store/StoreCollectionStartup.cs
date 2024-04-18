using Microsoft.Extensions.DependencyInjection;
using Toolbox.Tools;

namespace Toolbox.Store;

public static class StoreCollectionStartup
{
    public static IServiceCollection AddStoreCollection(this IServiceCollection services, Action<IServiceProvider, IStoreCollection> config)
    {
        services.NotNull();

        services.AddSingleton<IStoreCollection>(services =>
        {
            IStoreCollection storeCollection = ActivatorUtilities.CreateInstance<StoreCollection>(services);
            config(services, storeCollection);

            return storeCollection;
        });

        return services;
    }
}
