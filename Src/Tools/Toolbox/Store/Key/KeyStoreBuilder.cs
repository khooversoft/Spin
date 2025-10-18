using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Store;

public readonly struct FileSystemConfig<T>
{
    // Cache default delegates per closed generic T to avoid per-instance allocations.
    private static readonly Func<string, T?> s_defaultDeserializer = static s => s.ToObject<T>();
    private static readonly Func<T, string> s_defaultSerializer = static s => s.ToJson();

    // Ensure a parameterless construction path initializes non-null delegates.
    public FileSystemConfig()
    {
        BasePath = null;
        Deserialize = s_defaultDeserializer;
        Serialize = s_defaultSerializer;
    }

    public FileSystemConfig(string? basePath, Func<string, T?>? deserializer, Func<T, string>? serializer)
    {
        BasePath = basePath;
        Deserialize = deserializer ?? s_defaultDeserializer;
        Serialize = serializer ?? s_defaultSerializer;
    }

    public string? BasePath { get; }
    public Func<string, T?> Deserialize { get; }
    public Func<T, string> Serialize { get; }
}



public record KeyStoreBuilder<T>
{
    private readonly IList<Func<IServiceProvider, IKeyStore<T>>> _handlers = new List<Func<IServiceProvider, IKeyStore<T>>>();

    public KeyStoreBuilder(IServiceCollection services) => Services = services;
    public KeyStoreBuilder(IServiceCollection services, string? name) => (Services, KeyedName) = (services, name);

    public string? KeyedName { get; }
    public string? BasePath { get; set; } = null!;
    public Func<string, T?>? Deserializer { get; set; } = null!;
    public Func<T, string>? Serializer { get; set; } = null!;

    public IServiceCollection Services { get; }

    public void Add(Func<IServiceProvider, IKeyStore<T>> handler) => _handlers.Add(handler.NotNull());
    public void Add<TService>() where TService : class, IKeyStore<T> => _handlers.Add(service => GetService<TService>(service));
    public void AddSerializer(Func<T, string>? serializer) => Serializer = serializer;
    public void AddDeserializer(Func<string, T?>? deserializer) => Deserializer = deserializer;

    public FileSystemConfig<T> GetFileSystemConfig() => new(BasePath, Deserializer, Serializer);

    public IKeyStore<T> BuildHandlers(IServiceProvider services)
    {
        services.NotNull();
        bool hasKeyStore = false;

        IKeyStore<T>? firstHandler = _handlers
            .Reverse()
            .Aggregate((IKeyStore<T>?)null, (prev, current) =>
            {
                var handler = current(services);

                if (handler is KeyStore<T>)
                {
                    if (hasKeyStore) throw new InvalidOperationException("Multiple KeyStore<T> instances found. Only one is allowed.");
                    hasKeyStore = true;
                }

                handler.InnerHandler = prev;
                return handler;
            });

        var store = firstHandler switch
        {
            null => GetService<KeyStore<T>>(services),

            _ => hasKeyStore switch
            {
                true => firstHandler.Cast<IKeyStore<T>>(),
                false => addToEndOfChain(firstHandler),
            }
        };

        return store;

        IKeyStore<T> addToEndOfChain(IKeyStore<T> head)
        {
            getEnd(head).InnerHandler = GetService<KeyStore<T>>(services);
            return head;
        }

        IKeyStore<T> getEnd(IKeyStore<T> node)
        {
            while (node.InnerHandler != null) node = node.InnerHandler;
            return node.NotNull("Last handler was not found");
        }
    }

    private TService GetService<TService>(IServiceProvider services) where TService : notnull => KeyedName switch
    {
        null => services.GetRequiredService<TService>(),
        string v => CreateKeyedService<TService>(services),
    };

    private TService CreateKeyedService<TService>(IServiceProvider services) where TService : notnull
    {
        IFileSystem<T> fileSystem = services.GetRequiredKeyedService<IFileSystem<T>>(KeyedName);
        var subject = ActivatorUtilities.CreateInstance<TService>(services, fileSystem).NotNull("Failed to create service");
        return subject;
    }
}
