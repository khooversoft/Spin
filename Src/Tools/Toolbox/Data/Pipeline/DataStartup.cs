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
    public static IDataClient<T> GetDataClient<T>(this IServiceProvider serviceProvider, string pipelineName)
    {
        serviceProvider.NotNull();
        var factory = serviceProvider.GetRequiredService<DataClientFactory>();
        return factory.Create<T>(pipelineName);
    }

    public static IServiceCollection AddDataPipeline<T>(this IServiceCollection services, string pipelineName, Action<DataPipelineConfig> config)
    {
        services.NotNull();
        config.NotNull();
        pipelineName.NotEmpty();

        var builder = new DataPipelineConfig(services, pipelineName);
        config(builder);

        builder.FilePartitionStrategy ??= config => PartitionSchemas.ScalarFile(config.PipelineConfig, config.TypeName, config.Key);
        builder.ListPartitionStrategy ??= config => PartitionSchemas.DailyListPartitioning(config.PipelineConfig, config.TypeName, config.Key);
        builder.ListPartitionSearch ??= config => PartitionSchemas.DailyListPartitionSearch(config.PipelineConfig, config.TypeName, config.Key);

        builder.Validate().ThrowOnError();

        string keyedName = DataPipelineConfigTool.CreateKeyedName<T>(pipelineName);
        services.AddKeyedSingleton(keyedName, builder);
        services.TryAddSingleton<DataClientFactory>();

        builder.Services.AddTransient<IDataClient<T>>(serviceProvider =>
        {
            var factory = serviceProvider.GetRequiredService<DataClientFactory>();
            return factory.Create<T>(pipelineName);
        });

        return services;
    }

    public static DataPipelineConfig AddMemory(this DataPipelineConfig builder)
    {
        builder.NotNull();

        builder.Services.AddTransient<CacheMemoryHandler>();
        builder.Services.TryAddSingleton<IMemoryCache, MemoryCache>();

        builder.Handlers.Add<CacheMemoryHandler>();
        return builder;
    }

    public static DataPipelineConfig AddFileStore(this DataPipelineConfig builder)
    {
        builder.NotNull();

        builder.Services.AddTransient<FileStoreDataProvider>();
        builder.Handlers.Add<FileStoreDataProvider>();
        return builder;
    }

    public static DataPipelineConfig AddListStore(this DataPipelineConfig builder)
    {
        builder.NotNull();

        builder.Services.AddTransient<ListStoreDataProvider>();
        builder.Handlers.Add<ListStoreDataProvider>();
        return builder;
    }

    public static DataPipelineConfig AddQueueStore(this DataPipelineConfig builder)
    {
        builder.NotNull();

        builder.Services.AddTransient<QueueStoreHandler>();
        builder.Handlers.Add<QueueStoreHandler>();
        return builder;
    }

    public static DataPipelineConfig AddProvider(this DataPipelineConfig builder, Func<IServiceProvider, IDataProvider> config)
    {
        builder.NotNull();
        builder.Handlers.Add(service => config(service));
        return builder;
    }

    public static DataPipelineConfig AddProvider<T>(this DataPipelineConfig builder) where T : class, IDataProvider
    {
        builder.NotNull();

        builder.Services.AddTransient<T>();
        builder.Handlers.Add<T>();

        return builder;
    }
}