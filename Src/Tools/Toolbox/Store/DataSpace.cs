using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

/// <summary>
/// Path = "{storeName}:path"
/// 
/// Example:
///  "graph:databaseName"
///  "graphData:key/file.json"
///  "journal:journalName"
/// </summary>
public class DataSpace
{
    private readonly ConcurrentDictionary<string, SpaceDefinition> _spaces = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, IStoreProvider> _providers = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<DataSpace> _logger;

    public DataSpace(ILogger<DataSpace> logger) => _logger = logger.NotNull();

    public void AddSpace(DataSpaceOption definition)
    {
        definition.NotNull().Validate().ThrowOnError();

        foreach (var space in definition.Spaces)
        {
            _spaces.TryAdd(space.Name, space).BeTrue($"space={space.Name} already exists");
        }

        foreach (var provider in definition.Providers)
        {
            _providers.TryAdd(provider.Name, provider).BeTrue($"provider={provider.Name} already exists");
        }
    }


    public IKeyStore GetFileStore(string path)
    {
        (IStoreProvider provider, SpaceDefinition definition) = GetProvider(path);

        var keyStore = provider as IStoreKeyProvider ??
            throw new ArgumentException($"provider={definition.ProviderName} does not implement IStoreFileProvider");

        _logger.LogTrace("Getting file store for path={path}, provider={provider}", path, provider.Name);
        return keyStore.GetStore(definition).NotNull();
    }

    public IKeyStore<T> GetFileStore<T>(string path)
    {
        (IStoreProvider provider, SpaceDefinition definition) = GetProvider(path);

        var keyStore = provider as IStoreKeyProvider ??
            throw new ArgumentException($"provider={definition.ProviderName} does not implement IStoreFileProvider");

        _logger.LogTrace("Getting file store for path={path}, provider={provider}", path, provider.Name);
        return keyStore.GetStore<T>(definition).NotNull();
    }

    public IListStore<T> GetListStore<T>(string key)
    {
        (IStoreProvider provider, SpaceDefinition definition) = GetProvider(key);

        var keyStore = provider as IStoreListProvider ??
            throw new ArgumentException($"provider={definition.ProviderName} does not implement IStoreFileProvider");

        _logger.LogTrace("Getting list store for key={key}, provider={provider}", key, provider.Name);

        return keyStore.GetStore<T>(definition).NotNull();
    }

    public SpaceDefinition GetSpaceDefinition(string key)
    {
        string storeName = GetStoreName(key);
        _spaces.TryGetValue(storeName, out var definition).BeTrue($"storeName={storeName} not defined");
        return definition.NotNull();
    }

    private (IStoreProvider storeProvider, SpaceDefinition definition) GetProvider(string key)
    {
        SpaceDefinition definition = GetSpaceDefinition(key);
        _providers.TryGetValue(definition.NotNull().ProviderName, out var provider).BeTrue($"provider={definition.ProviderName} not registered");

        return (provider.NotNull(), definition);
    }

    private static string GetStoreName(string path)
    {
        path.NotEmpty();

        string storeName = path.IndexOf(':') switch
        {
            -1 => path,
            0 => throw new ArgumentException($"Invalid path format, missing store name in path={path}"),
            int idx => path[..idx],
        };

        return storeName;
    }
}
