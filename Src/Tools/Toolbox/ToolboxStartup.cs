using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox;

public static class ToolboxStartup
{
    public static IServiceCollection AddInMemoryFileStore(this IServiceCollection services, MemoryStore? memoryStore = null)
    {
        services.NotNull();

        services.AddSingleton<IFileStore, InMemoryFileStore>();

        switch (memoryStore)
        {
            case null: services.AddSingleton<MemoryStore>(); break;
            default: services.AddSingleton(memoryStore); break;
        }

        return services;
    }

    public static IServiceCollection AddListStore<T>(this IServiceCollection services, Action<ListStoreBuilder<T>>? config = null)
    {
        services.NotNull();

        var builder = new ListStoreBuilder<T>(services);
        config?.Invoke(builder);

        services.TryAddSingleton<IListFileSystem<T>>(services => builder.BasePath switch
        {
            null => ActivatorUtilities.CreateInstance<ListFileSystem<T>>(services),
            string basePath => ActivatorUtilities.CreateInstance<ListFileSystem<T>>(services, basePath),
        });

        services.AddSingleton<ListStore<T>>();

        services.AddSingleton<IListStore<T>>(services =>
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

        builder.Services.AddSingleton<ListBatchProvider<T>>(services =>
                ActivatorUtilities.CreateInstance<ListBatchProvider<T>>(services, batchInterval.Value)
            );

        builder.NotNull().Add<ListBatchProvider<T>>();
        return builder;
    }
}
