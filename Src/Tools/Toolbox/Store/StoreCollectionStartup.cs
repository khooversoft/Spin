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
            IStoreCollection storeCollection = new StoreCollection(services);
            config(services, storeCollection);

            return storeCollection;
        });

        return services;
    }
}
