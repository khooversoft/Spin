using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox;

public static class KeyStoreStartup
{
    public static IServiceCollection AddKeyStore<T>(this IServiceCollection services, FileSystemType fileSystemType, Action<KeyStoreBuilder<T>>? config = null)
    {
        services.NotNull();

        var builder = new KeyStoreBuilder<T>(services);
        config?.Invoke(builder);

        switch (fileSystemType)
        {
            case FileSystemType.Key:
                services.TryAddSingleton<IFileSystem<T>>(services => builder.BasePath switch
                {
                    null => ActivatorUtilities.CreateInstance<KeyFileSystem<T>>(services),
                    string basePath => ActivatorUtilities.CreateInstance<KeyFileSystem<T>>(services, basePath),
                });
                break;

            case FileSystemType.Hash:
                services.TryAddSingleton<IFileSystem<T>>(services => builder.BasePath switch
                {
                    null => ActivatorUtilities.CreateInstance<HashFileSystem<T>>(services),
                    string basePath => ActivatorUtilities.CreateInstance<HashFileSystem<T>>(services, basePath),
                });
                break;
        }

        services.AddTransient<KeyStore<T>>();
        services.AddSingleton<LockManager>();

        services.AddTransient<IKeyStore<T>>(services =>
        {
            IKeyStore<T> keyStore = services.GetRequiredService<KeyStore<T>>();
            var keyStoreClient = builder.BuildHandlers(services, keyStore) switch
            {
                { StatusCode: StatusCode.OK } v => v.Return(),
                _ => keyStore,
            };

            return keyStoreClient;
        });

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
                ActivatorUtilities.CreateInstance<KeyCacheProvider<T>>(services, cacheSpan.Value)
            );

        builder.Services.TryAddSingleton<IMemoryCache, MemoryCache>();
        builder.NotNull().Add<KeyCacheProvider<T>>();
        return builder;
    }

    public static KeyStoreBuilder<T> AddLockProvider<T>(this KeyStoreBuilder<T> builder, LockMode lockMode)
    {
        builder.Services.AddTransient<KeyLockProvider<T>>(services =>
                ActivatorUtilities.CreateInstance<KeyLockProvider<T>>(services, lockMode)
            );

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
