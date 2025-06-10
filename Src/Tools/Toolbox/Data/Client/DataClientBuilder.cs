using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Tools;

namespace Toolbox.Store;

public class DataClientBuilder
{
    public DataClientBuilder(IServiceCollection services) => Services = services.NotNull();

    public IServiceCollection Services { get; }
    public string? Name { get; set; }
    public TimeSpan MemoryCacheDuration { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan FileCacheDuration { get; set; } = TimeSpan.FromDays(5);
    public IList<Func<IServiceProvider, DataClientHandler>> Handlers { get; } = new List<Func<IServiceProvider, DataClientHandler>>();
}


public static class DataClientBuilderExtensions
{
    public static IDataClient GetDataClient(this IServiceProvider serviceProvider, string name = "default")
    {
        serviceProvider.NotNull();
        name.NotEmpty();
        var factory = serviceProvider.GetRequiredService<DataClientFactory>();
        return factory.Create(name);
    }

    public static IDataClient<T> GetDataClient<T>(this IServiceProvider serviceProvider)
    {
        serviceProvider.NotNull();
        var factory = serviceProvider.GetRequiredService<DataClientFactory>();
        return factory.Create<T>();
    }

    public static DataClientBuilder AddMemoryCache(this DataClientBuilder builder)
    {
        builder.NotNull();

        builder.Services.TryAddTransient<DataClientHandler>();
        builder.Services.TryAddSingleton<CacheMemoryDataProvider>();
        builder.Services.TryAddSingleton<IMemoryCache, MemoryCache>();

        builder.Handlers.Add(service =>
        {
            IDataProvider provider = service.GetRequiredService<CacheMemoryDataProvider>();
            var handler = ActivatorUtilities.CreateInstance<DataClientHandler>(service, provider);
            return handler;
        });

        return builder;
    }

    public static DataClientBuilder AddFileStoreCache(this DataClientBuilder builder)
    {
        builder.NotNull();

        builder.Services.TryAddTransient<DataClientHandler>();
        builder.Services.TryAddSingleton<CacheFileStoreDataProvider>();

        builder.Handlers.Add(service =>
        {
            IDataProvider provider = service.GetRequiredService<CacheFileStoreDataProvider>();
            var handler = ActivatorUtilities.CreateInstance<DataClientHandler>(service, provider);
            return handler;
        });

        return builder;
    }

    public static DataClientBuilder AddProvider(this DataClientBuilder builder, IDataProvider customProvider)
    {
        builder.NotNull();
        customProvider.NotNull();

        builder.Handlers.Add(service =>
        {
            var handler = ActivatorUtilities.CreateInstance<DataClientHandler>(service, customProvider);
            return handler;
        });

        return builder;
    }

    public static DataClientBuilder AddProvider<T>(this DataClientBuilder builder) where T : class, IDataProvider
    {
        builder.NotNull();
        builder.Services.AddSingleton<T>();

        builder.Services.Configure<DataClientBuilder>(builder.Name, (DataClientBuilder option) =>
        {
            option.Handlers.Add(service =>
            {
                IDataProvider provider = service.GetRequiredService<T>();
                var handler = ActivatorUtilities.CreateInstance<DataClientHandler>(service, provider);
                return handler;
            });
        });

        return builder;
    }
}