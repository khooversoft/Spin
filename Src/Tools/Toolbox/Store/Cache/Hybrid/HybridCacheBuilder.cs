using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Tools;

namespace Toolbox.Store;

public class HybridCacheBuilder
{
    public HybridCacheBuilder(IServiceCollection services) => Services = services.NotNull();

    public IServiceCollection Services { get; }
    public string? Name { get; set; }
    public TimeSpan MemoryCacheDuration { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan FileCacheDuration { get; set; } = TimeSpan.FromDays(5);
    public IList<Func<IServiceProvider, HybridCacheHandler>> Handlers { get; } = new List<Func<IServiceProvider, HybridCacheHandler>>();
}


public static class HybridCacheBuilderExtensions
{
    public static IHybridCache GetHybridCache(this IServiceProvider serviceProvider, string name = "default")
    {
        serviceProvider.NotNull();
        name.NotEmpty();
        var factory = serviceProvider.GetRequiredService<HybridCacheFactory>();
        return factory.Create(name);
    }

    public static IHybridCache<T> GetHybridCache<T>(this IServiceProvider serviceProvider)
    {
        serviceProvider.NotNull();
        var factory = serviceProvider.GetRequiredService<HybridCacheFactory>();
        return factory.Create<T>();
    }

    public static HybridCacheBuilder AddMemoryCache(this HybridCacheBuilder builder)
    {
        builder.NotNull();

        builder.Services.TryAddTransient<HybridCacheHandler>();
        builder.Services.TryAddSingleton<HybridCacheMemoryProvider>();
        builder.Services.TryAddSingleton<IMemoryCache, MemoryCache>();

        builder.Handlers.Add(service =>
        {
            IHybridCacheProvider provider = service.GetRequiredService<HybridCacheMemoryProvider>();
            var handler = ActivatorUtilities.CreateInstance<HybridCacheHandler>(service, provider);
            return handler;
        });

        return builder;
    }

    public static HybridCacheBuilder AddFileStoreCache(this HybridCacheBuilder builder)
    {
        builder.NotNull();

        builder.Services.TryAddTransient<HybridCacheHandler>();
        builder.Services.TryAddSingleton<HybridCacheFileStoreProvider>();

        builder.Handlers.Add(service =>
        {
            IHybridCacheProvider provider = service.GetRequiredService<HybridCacheFileStoreProvider>();
            var handler = ActivatorUtilities.CreateInstance<HybridCacheHandler>(service, provider);
            return handler;
        });

        return builder;
    }

    public static HybridCacheBuilder AddProvider(this HybridCacheBuilder builder, IHybridCacheProvider customProvider)
    {
        builder.NotNull();
        customProvider.NotNull();
        //builder.Services.AddSingleton<IHybridCacheProvider>(customProvider);

        builder.Handlers.Add(service =>
        {
            var handler = ActivatorUtilities.CreateInstance<HybridCacheHandler>(service, customProvider);
            return handler;
        });

        return builder;
    }

    public static HybridCacheBuilder AddProvider<T>(this HybridCacheBuilder builder) where T : class, IHybridCacheProvider
    {
        builder.NotNull();
        builder.Services.AddSingleton<T>();
        builder.Services.AddTransient(typeof(HybridCache<>));

        builder.Services.Configure<HybridCacheBuilder>(builder.Name, (HybridCacheBuilder option) =>
        {
            option.Handlers.Add(service =>
            {
                IHybridCacheProvider provider = service.GetRequiredService<T>();
                var handler = ActivatorUtilities.CreateInstance<HybridCacheHandler>(service, provider);
                return handler;
            });
        });

        return builder;
    }
}