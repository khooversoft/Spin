using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Store;
using Toolbox.Tools;

namespace Toolbox;

public static class KeyStoreStartup
{
    public static IServiceCollection AddKeyStore<T>(this IServiceCollection services, FileSystemType fileSystemType, Action<KeyStoreBuilder<T>>? config = null)
    {
        services.NotNull();

        var builder = new KeyStoreBuilder<T>(services);
        config?.Invoke(builder);
        var fileSystemConfig = builder.GetFileSystemConfig();

        switch (fileSystemType)
        {
            case FileSystemType.Key:
                services.TryAddSingleton<IFileSystem<T>>(services => ActivatorUtilities.CreateInstance<KeyFileSystem<T>>(services, fileSystemConfig));
                break;

            case FileSystemType.Hash:
                services.TryAddSingleton<IFileSystem<T>>(services => ActivatorUtilities.CreateInstance<HashFileSystem<T>>(services, fileSystemConfig));
                break;

            default: throw new ArgumentException($"Unsupported FileSystemType: {fileSystemType}", nameof(fileSystemType));
        }

        services.TryAddTransient<KeyStore<T>>();
        services.TryAddSingleton<LockManager>();
        services.AddTransient<IKeyStore<T>>(builder.BuildHandlers);

        return services;
    }

    public static IServiceCollection AddKeyedKeyStore<T>(this IServiceCollection services, FileSystemType fileSystemType, string name, Action<KeyStoreBuilder<T>>? config = null)
    {
        services.NotNull();
        name.NotEmpty();

        var builder = new KeyStoreBuilder<T>(services, name);
        config?.Invoke(builder);
        var fileSystemConfig = builder.GetFileSystemConfig();

        switch (fileSystemType)
        {
            case FileSystemType.Key:
                services.TryAddKeyedSingleton<IFileSystem<T>>(name, (services, _) => ActivatorUtilities.CreateInstance<KeyFileSystem<T>>(services, fileSystemConfig));
                break;

            case FileSystemType.Hash:
                services.TryAddKeyedSingleton<IFileSystem<T>>(name, (services, _) => ActivatorUtilities.CreateInstance<HashFileSystem<T>>(services, fileSystemConfig));
                break;

            default: throw new ArgumentException($"Unsupported FileSystemType: {fileSystemType}", nameof(fileSystemType));
        }

        services.AddKeyedTransient<KeyStore<T>>(name);
        services.TryAddSingleton<LockManager>();
        services.AddKeyedTransient<IKeyStore<T>>(name, (services, _) => builder.BuildHandlers(services));

        return services;
    }

    public static KeyStoreBuilder<T> AddKeyStore<T>(this KeyStoreBuilder<T> builder)
    {
        builder.NotNull().Add<KeyStore<T>>();
        return builder;
    }

    public static KeyStoreBuilder<T> AddCacheProvider<T>(this KeyStoreBuilder<T> builder, TimeSpan? cacheSpan = null)
    {
        cacheSpan ??= TimeSpan.FromSeconds(1);

        builder.Services.AddTransient<KeyCacheProvider<T>>(services =>
            ActivatorUtilities.CreateInstance<KeyCacheProvider<T>>(services, cacheSpan.Value));

        builder.Services.TryAddSingleton<IMemoryCache, MemoryCache>();
        builder.NotNull().Add<KeyCacheProvider<T>>();
        return builder;
    }

    public static KeyStoreBuilder<T> AddLockProvider<T>(this KeyStoreBuilder<T> builder, LockMode lockMode)
    {
        builder.Services.AddTransient<KeyLockProvider<T>>(services =>
            ActivatorUtilities.CreateInstance<KeyLockProvider<T>>(services, lockMode));

        builder.NotNull().Add<KeyLockProvider<T>>();
        return builder;
    }

    public static KeyStoreBuilder<T> AddCustomProvider<T>(this KeyStoreBuilder<T> builder, Func<IServiceProvider, IKeyStore<T>> provider)
    {
        provider.NotNull();
        builder.NotNull().Add(provider);
        return builder;
    }

    public static KeyStoreBuilder<T> AddCustomProvider<T, TProvider>(this KeyStoreBuilder<T> builder) where TProvider : class, IKeyStore<T>
    {
        builder.NotNull().Add<TProvider>();
        return builder;
    }
}
