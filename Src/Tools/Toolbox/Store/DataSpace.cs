using System.Collections.Frozen;
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
    private readonly FrozenDictionary<string, SpaceDefinition> _spaces;
    private readonly FrozenDictionary<string, IStoreProvider> _providers;
    private readonly ILogger<DataSpace> _logger;

    public DataSpace(DataSpaceOption option, ILogger<DataSpace> logger)
    {
        option.NotNull().Validate().ThrowOnError();
        _logger = logger.NotNull();

        _spaces = option.Spaces
            .Select(x => new KeyValuePair<string, SpaceDefinition>(x.Name, x))
            .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        _providers = option.Providers
            .Select(x => new KeyValuePair<string, IStoreProvider>(x.Name, x))
            .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    public IKeyStore GetFileStore(string path)
    {
        (IStoreProvider provider, SpaceDefinition definition) = GetProvider(path);

        var keyStore = provider as IStoreKeyProvider ??
            throw new ArgumentException($"provider={definition.ProviderName} does not implement IStoreFileProvider");

        _logger.LogTrace("Getting file store for path={path}, provider={provider}", path, provider.Name);
        return keyStore.GetStore(definition).NotNull();
    }

    public IListStore<T> GetListStore<T>(string key)
    {
        (IStoreProvider provider, SpaceDefinition definition) = GetProvider(key);

        var keyStore = provider as IStoreListProvider ??
            throw new ArgumentException($"provider={definition.ProviderName} does not implement IStoreFileProvider");

        _logger.LogTrace("Getting list store for key={key}, provider={provider}", key, provider.Name);
        return keyStore.GetStore<T>(definition).NotNull();
    }

    private (IStoreProvider storeProvider, SpaceDefinition definition) GetProvider(string path)
    {
        string storeName = GetStoreName(path);

        _spaces.TryGetValue(storeName, out var definition).BeTrue($"storeName={storeName} not defined");
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
