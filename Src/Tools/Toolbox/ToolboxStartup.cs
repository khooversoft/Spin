using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Store;
using Toolbox.Tools;

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

    public static IServiceCollection AddInMemoryKeyStore(this IServiceCollection services, MemoryStore? memoryStore = null)
    {
        services.NotNull();

        services.AddSingleton<IKeyStore, MemoryKeyStore>();

        switch (memoryStore)
        {
            case null: services.AddSingleton<MemoryStore>(); break;
            default: services.AddSingleton(memoryStore); break;
        }

        return services;
    }

    public static IServiceCollection AddDataSpace(this IServiceCollection services, Action<DataSpaceConfig> config)
    {
        services.NotNull();
        config.NotNull();

        var c = new DataSpaceConfig();
        config(c);

        services.TryAddSingleton<AccessManager>();

        services.AddSingleton<DataSpace>(services =>
        {
            var option = c.Build(services);
            return ActivatorUtilities.CreateInstance<DataSpace>(services, option);
        });

        return services;
    }

    public static IServiceCollection AddKeyStore(this IServiceCollection services, string spaceName)
    {
        services.NotNull();
        spaceName.NotEmpty();

        services.AddKeyedTransient<IKeyStore>(spaceName, (services, k) =>
        {
            var dataSpace = services.GetRequiredService<DataSpace>();
            return dataSpace.GetFileStore(spaceName);
        });

        return services;
    }

    public static IServiceCollection AddListStore<T>(this IServiceCollection services, string spaceName)
    {
        services.NotNull();
        spaceName.NotEmpty();

        services.AddTransient<IListStore2<T>>(services =>
        {
            var dataSpace = services.GetRequiredService<DataSpace>();
            return dataSpace.GetListStore<T>(spaceName);
        });

        return services;
    }
}
