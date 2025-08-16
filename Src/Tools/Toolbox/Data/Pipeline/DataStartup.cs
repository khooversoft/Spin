//using System.Runtime.CompilerServices;
//using Microsoft.Extensions.Caching.Memory;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.DependencyInjection.Extensions;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;

//[assembly: InternalsVisibleTo("Toolbox.Test")]

//namespace Toolbox.Data;

//public static class DataStartup
//{
//    public static IDataClient<T> GetDataClient<T>(this IServiceProvider serviceProvider) => serviceProvider.NotNull().GetRequiredService<IDataClient<T>>();

//    public static IDataListClient<T> GetDataListClient<T>(this IServiceProvider serviceProvider) => serviceProvider.NotNull().GetRequiredService<IDataListClient<T>>();

//    public static IServiceCollection AddDataPipeline<T>(this IServiceCollection services, Action<IDataPipelineBuilder> config)
//    {
//        services.NotNull();
//        config.NotNull();

//        var builder = new DataPipelineConfig<T>(services);
//        config(builder);
//        //builder.FileSystem ??= new HashFileSystem("fake");
//        builder.Validate().ThrowOnError();

//        services.AddSingleton(builder);
//        services.AddSingleton<LockManager>();
//        builder.Services.AddTransient<IDataClient<T>, DataClient<T>>();

//        return services;
//    }

//    public static IServiceCollection AddDataListPipeline<T>(this IServiceCollection services, Action<IDataPipelineBuilder> config)
//    {
//        services.NotNull();
//        config.NotNull();

//        var builder = new DataPipelineConfig<T>(services);
//        config(builder);
//        //builder.FileSystem ??= new ListFileSystem<T>("fake");
//        builder.Validate().ThrowOnError();

//        services.AddSingleton(builder);
//        services.AddSingleton<LockManager>();
//        builder.Services.AddTransient<IDataListClient<T>, DataListClient<T>>();

//        return services;
//    }

//    public static IDataPipelineBuilder AddCacheMemory(this IDataPipelineBuilder builder)
//    {
//        builder.NotNull();

//        builder.Services.AddTransient<CacheMemoryHandler>();
//        builder.Services.TryAddSingleton<IMemoryCache, MemoryCache>();

//        builder.Handlers.Add<CacheMemoryHandler>();
//        return builder;
//    }

//    public static IDataPipelineBuilder AddFileStore(this IDataPipelineBuilder builder)
//    {
//        builder.NotNull();

//        builder.Services.AddTransient<FileStoreDataProvider>();
//        builder.Handlers.Add<FileStoreDataProvider>();
//        return builder;
//    }

//    public static IDataPipelineBuilder AddFileLocking(this IDataPipelineBuilder builder, Action<DataPipelineLockConfig> config)
//    {
//        builder.NotNull();
//        config.NotNull();

//        var lockConfig = new DataPipelineLockConfig();
//        config(lockConfig);

//        builder.Services.TryAddSingleton<LockManager>();
//        builder.Services.AddTransient<FileStoreLockHandler>();

//        builder.Handlers.Add(service =>
//        {
//            var lockHandler = ActivatorUtilities.CreateInstance<FileStoreLockHandler>(service, lockConfig);
//            return lockHandler;
//        });

//        return builder;
//    }

//    public static IDataPipelineBuilder AddListStore(this IDataPipelineBuilder builder)
//    {
//        builder.NotNull();

//        builder.Services.AddTransient<ListStoreDataProvider>();
//        builder.Handlers.Add<ListStoreDataProvider>();
//        return builder;
//    }

//    public static IDataPipelineBuilder AddQueueStore(this IDataPipelineBuilder builder)
//    {
//        builder.NotNull();

//        builder.Services.AddTransient<QueueStoreHandler>();
//        builder.Handlers.Add<QueueStoreHandler>();
//        return builder;
//    }

//    public static IDataPipelineBuilder AddProvider(this IDataPipelineBuilder builder, Func<IServiceProvider, IDataProvider> config)
//    {
//        builder.NotNull();
//        builder.Handlers.Add(service => config(service));
//        return builder;
//    }

//    public static IDataPipelineBuilder AddProvider<T>(this IDataPipelineBuilder builder) where T : class, IDataProvider
//    {
//        builder.NotNull();

//        builder.Services.AddTransient<T>();
//        builder.Handlers.Add<T>();

//        return builder;
//    }
//}

