using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox;

public static class ListStoreStartup
{
    public static IServiceCollection AddListStore<T>(this IServiceCollection services, Action<ListStoreBuilder<T>>? config = null)
    {
        services.NotNull();

        var builder = new ListStoreBuilder<T>(services);
        config?.Invoke(builder);
        var fileSystemConfig = builder.GetFileSystemConfig();

        services.TryAddTransient<IListFileSystem<T>>(services => ActivatorUtilities.CreateInstance<ListFileSystem<T>>(services, fileSystemConfig));
        services.AddTransient<ListStore<T>>();
        services.AddTransient<IListStore<T>>(services =>
        {
            IListStore<T> listStore = services.GetRequiredService<ListStore<T>>();

            var listStoreClient = builder.BuildHandlers(services, listStore) switch
            {
                { StatusCode: StatusCode.OK } v => v.Return(),
                _ => listStore,
            };

            return listStoreClient;
        });

        return services;
    }

    public static ListStoreBuilder<T> AddBatchProvider<T>(this ListStoreBuilder<T> builder, TimeSpan? batchInterval = null)
    {
        batchInterval ??= TimeSpan.FromSeconds(1);

        builder.Services.AddTransient<ListBatchProvider<T>>(services =>
                ActivatorUtilities.CreateInstance<ListBatchProvider<T>>(services, batchInterval.Value)
            );

        builder.NotNull().Add<ListBatchProvider<T>>();
        return builder;
    }
}
