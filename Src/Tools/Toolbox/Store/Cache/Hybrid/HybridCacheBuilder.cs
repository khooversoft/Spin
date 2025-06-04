using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Tools;

namespace Toolbox.Store;

public readonly struct HybridCacheBuilder
{
    public HybridCacheBuilder(IServiceCollection services)
    {
        services.NotNull();
        Services = services;
    }

    public IServiceCollection Services { get; }
}

public static class HybridCacheBuilderExtensions
{
    public static HybridCacheBuilder AddMemoryCache(this HybridCacheBuilder builder)
    {
        builder.NotNull();

        builder.Services.AddSingleton<IHybridCacheProvider, HybridCacheMemoryProvider>();
        builder.Services.TryAddSingleton<IMemoryCache, MemoryCache>();

        return builder;
    }

    public static HybridCacheBuilder AddFileStoreCache(this HybridCacheBuilder builder)
    {
        builder.NotNull();

        builder.Services.AddSingleton<IHybridCacheProvider, HybridCacheFileStoreProvider>();
        return builder;
    }

    public static HybridCacheBuilder AddProvider<T>(this HybridCacheBuilder builder) where T : class, IHybridCacheProvider
    {
        builder.NotNull();
        builder.Services.AddSingleton<T>();
        return builder;
    }

    public static HybridCacheBuilder AddProvider<T>(this HybridCacheBuilder builder, T value) where T : class, IHybridCacheProvider
    {
        builder.NotNull();
        builder.Services.AddSingleton<T>(value);
        return builder;
    }
}