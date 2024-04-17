using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public interface IStoreCollection
{
    IStoreCollection Add(StoreConfig storeConfig);
    IFileStore Get(string alias);
    (string alias, string filePath) GetAliasAndPath(string path, string extension);
}

public class StoreCollection : IStoreCollection
{
    private readonly ConcurrentDictionary<string, StoreConfig> _stores = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, IFileStore> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly IServiceProvider _serviceProvider;

    public StoreCollection(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider.NotNull();

    public IStoreCollection Add(StoreConfig storeConfig)
    {
        storeConfig.NotNull();

        _stores.TryAdd(storeConfig.Alias, storeConfig).Assert(x => x == true, $"Could not add config={storeConfig}");
        return this;
    }

    public IFileStore Get(string alias)
    {
        alias.NotEmpty();

        var fileStore = _cache.GetOrAdd(alias, _ =>
        {
            _stores.TryGetValue(alias, out StoreConfig? storeConfig).Assert(x => x == true, $"Alias={alias} not found");

            var createFileStore = storeConfig!.Create(_serviceProvider, storeConfig);
            return createFileStore;
        });

        return fileStore;
    }

    public (string alias, string filePath) GetAliasAndPath(string path, string extension)
    {
        string[] parts = path
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .ToArray();

        (string alias, string path) result = parts switch
        {
            { Length: 1 } => (parts[0], parts[0]),
            { Length: > 1 } => (parts[0], setExtensionIfRequired(parts.Skip(1).Join('/'))),
            _ => throw new ArgumentException($"Invalid grainId={path}"),
        };

        return result;

        string setExtensionIfRequired(string path) => extension.IsEmpty() ? path : PathTool.SetExtension(path, extension);
    }
}
