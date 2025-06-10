using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Journal;
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
        ;

        return services;
    }

    public static IServiceCollection AddLocalFileStore(this IServiceCollection services, LocalFileStoreOption option)
    {
        services.NotNull();
        option.NotNull().Validate().ThrowOnError("Invalid LocalFileStoreOption");

        services.AddSingleton(option);
        //services.AddSingleton<IFileStore, LocalFileStore>();
        return services;
    }

    public static IServiceCollection AddJournalLog(this IServiceCollection services, string key, JournalFileOption option)
    {
        services.NotNull();
        key.NotEmpty();
        option.NotNull().Validate().ThrowOnError();

        services.AddSingleton(option);
        services.AddKeyedSingleton<IJournalFile, JournalFile>(key);

        return services;
    }

    public static IServiceCollection AddHybridCache(this IServiceCollection services, Action<HybridCacheBuilder> config, string? name = "default")
    {
        services.NotNull();
        config.NotNull();

        var builder = new HybridCacheBuilder(services) { Name = name };
        config(builder);
        builder.Name.NotEmpty("Name is required");

        services.TryAddSingleton<HybridCacheFactory>();
        services.AddKeyedSingleton(builder.Name, builder);
        return services;
    }

    public static IServiceCollection AddHybridCache<T>(this IServiceCollection services, Action<HybridCacheBuilder>? config = null)
    {
        services.NotNull();
        config.NotNull();

        var builder = new HybridCacheBuilder(services) { Name = typeof(T).Name };
        config(builder);

        services.TryAddSingleton<HybridCacheFactory>();
        services.AddKeyedSingleton(builder.Name, builder);

        builder.Services.AddTransient<IHybridCache<T>>(serviceProvider =>
        {
            var factory = serviceProvider.GetRequiredService<HybridCacheFactory>();
            return factory.Create<T>();
        });

        return services;
    }
}
