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

        var builder = new DataPipelineConfig(services, pipelineName, DataPipelineConfigTool.CreateKeyedName<T>(pipelineName));
        config(builder);

        builder.PartitionStrategy ??= new PartitionStrategy();
        builder.Validate().ThrowOnError();

        services.AddKeyedSingleton(builder.ServiceKeyedName, builder);
        services.TryAddSingleton<DataClientFactory>();

        builder.Services.AddTransient<IDataClient<T>>(serviceProvider =>
        {
            var factory = serviceProvider.GetRequiredService<DataClientFactory>();
            return factory.Create<T>(pipelineName);
        });

        return services;
    }

    public static DataPipelineConfig AddCacheMemory(this DataPipelineConfig builder)
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

    public static DataPipelineConfig AddFileLocking(this DataPipelineConfig builder, Action<DataPipelineLockConfig> config)
    {
        builder.NotNull();

        var lockConfig = new DataPipelineLockConfig();
        config.NotNull()(lockConfig);
        builder.Validate().ThrowOnError();
        builder.Services.AddKeyedSingleton<DataPipelineLockConfig>(builder.ServiceKeyedName, lockConfig);

        builder.Services.TryAddSingleton<LockDetailCollection>();
        builder.Services.AddTransient<FileStoreLockHandler>();

        builder.Handlers.Add(service =>
        {
            var lockConfig = service.GetRequiredKeyedService<DataPipelineLockConfig>(builder.ServiceKeyedName);
            var lockHandler = ActivatorUtilities.CreateInstance<FileStoreLockHandler>(service, lockConfig);
            return lockHandler;
        });

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