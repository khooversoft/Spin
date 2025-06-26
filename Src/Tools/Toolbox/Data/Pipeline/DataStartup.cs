using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

[assembly: InternalsVisibleTo("Toolbox.Test")]

namespace Toolbox.Data;

public static class DataStartup
{
    public static IServiceCollection AddDataPipeline<T>(this IServiceCollection services, Action<DataPipelineBuilder> config, string pipelineName)
    {
        services.NotNull();
        config.NotNull();
        pipelineName.NotEmpty();

        var builder = new DataPipelineBuilder(services, pipelineName);
        config(builder);
        builder.Validate().ThrowOnError();

        string keyedName = DataPipelineBuilderTool.CreateKeyedName<T>(pipelineName);
        services.AddKeyedSingleton(keyedName, builder);

        services.TryAddSingleton<DataClientFactory>();

        builder.Services.AddTransient<IDataClient<T>>(serviceProvider =>
        {
            var factory = serviceProvider.GetRequiredService<DataClientFactory>();
            return factory.Create<T>(pipelineName);
        });

        return services;
    }

    public static IServiceCollection AddJournalPipeline<T>(this IServiceCollection services, Action<DataPipelineBuilder> config, string pipelineName)
    {
        services.NotNull();
        config.NotNull();
        pipelineName.NotEmpty();

        pipelineName ??= typeof(T).Name;
        var builder = new DataPipelineBuilder(services, pipelineName);
        config(builder);
        builder.Validate().ThrowOnError();

        string keyedName = DataPipelineBuilderTool.CreateKeyedName<T>(pipelineName);
        services.AddKeyedSingleton(keyedName, builder);

        services.TryAddSingleton<JournalClientFactory>();

        builder.Services.AddTransient<IJournalClient<T>>(serviceProvider =>
        {
            var factory = serviceProvider.GetRequiredService<JournalClientFactory>();
            return factory.Create<T>(pipelineName);
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