using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Tools;

namespace Toolbox.Data;

public static class DataStartup
{
    public static IServiceCollection AddDataPipeline(this IServiceCollection services, Action<DataPipelineBuilder> config, string? name = "default")
    {
        services.NotNull();
        config.NotNull();

        var builder = new DataPipelineBuilder(services) { Name = name };
        config(builder);
        builder.Name.NotEmpty("Name is required");

        services.TryAddSingleton<DataClientFactory>();
        services.AddKeyedSingleton(builder.Name, builder);
        return services;
    }

    public static IServiceCollection AddDataPipeline<T>(this IServiceCollection services, Action<DataPipelineBuilder>? config = null)
    {
        services.NotNull();
        config.NotNull();

        var builder = new DataPipelineBuilder(services) { Name = typeof(T).Name };
        config(builder);

        services.TryAddSingleton<DataClientFactory>();
        services.AddKeyedSingleton(builder.Name, builder);

        builder.Services.AddTransient<IDataClient>(serviceProvider =>
        {
            var factory = serviceProvider.GetRequiredService<DataClientFactory>();
            return factory.Create("default");
        });

        builder.Services.AddTransient<IDataClient<T>>(serviceProvider =>
        {
            var factory = serviceProvider.GetRequiredService<DataClientFactory>();
            return factory.Create<T>();
        });

        return services;
    }

    public static IServiceCollection AddJournalPipeline<T>(this IServiceCollection services, Action<DataPipelineBuilder> config)
    {
        services.NotNull();
        config.NotNull();

        var builder = new DataPipelineBuilder(services) { Name = typeof(T).Name };
        config(builder);

        services.TryAddSingleton<JournalClientFactory>();
        services.AddKeyedSingleton(builder.Name, builder);

        builder.Services.AddTransient<IJournalClient<T>>(serviceProvider =>
        {
            var factory = serviceProvider.GetRequiredService<JournalClientFactory>();
            return factory.Create<T>();
        });

        return services;
    }

    public static DataPipelineBuilder AddMemory(this DataPipelineBuilder builder)
    {
        builder.NotNull();

        builder.Services.AddTransient<CacheMemoryDataProvider>();
        builder.Services.TryAddSingleton<IMemoryCache, MemoryCache>();

        builder.Handlers.Add<CacheMemoryDataProvider>();
        return builder;
    }

    public static DataPipelineBuilder AddFileStore(this DataPipelineBuilder builder)
    {
        builder.NotNull();

        builder.Services.AddTransient<FileStoreDataProvider>();
        builder.Handlers.Add<FileStoreDataProvider>();
        return builder;
    }

    public static DataPipelineBuilder AddJournalStore(this DataPipelineBuilder builder)
    {
        builder.NotNull();

        builder.Services.AddTransient<JournalStoreDataProvider>();
        builder.Handlers.Add<JournalStoreDataProvider>();
        return builder;
    }

    public static DataPipelineBuilder AddProvider(this DataPipelineBuilder builder, Func<IServiceProvider, IDataProvider> config)
    {
        builder.NotNull();
        builder.Handlers.Add(service => config(service));
        return builder;
    }

    public static DataPipelineBuilder AddProvider<T>(this DataPipelineBuilder builder) where T : class, IDataProvider
    {
        builder.NotNull();

        builder.Services.AddTransient<T>();
        builder.Handlers.Add<T>();

        return builder;
    }
}