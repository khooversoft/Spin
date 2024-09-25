using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public interface IStoreCollection
{
    IStoreCollection Add(StoreConfig storeConfig);
    IStoreCollection Add(string alias, Func<IServiceProvider, StoreConfig, IFileStore> create);
    IFileStore Get(string alias);
    (string alias, string filePath) GetAliasAndPath(string path, string? extension = null);
}

public class StoreCollection : IStoreCollection
{
    private readonly ConcurrentDictionary<string, StoreConfig> _stores = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, IFileStore> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StoreCollection> _logger;
    private readonly ScopeContext _context;

    public StoreCollection(IServiceProvider serviceProvider, ILogger<StoreCollection> logger)
    {
        _serviceProvider = serviceProvider.NotNull();
        _logger = logger.NotNull();

        _context = new ScopeContext(_logger);
    }

    public IStoreCollection Add(string alias, Func<IServiceProvider, StoreConfig, IFileStore> create) => Add(new StoreConfig(alias, create));

    public IStoreCollection Add(StoreConfig storeConfig)
    {
        storeConfig.NotNull();

        _context.LogInformation("Adding store configuration, alias={alias}", storeConfig.Alias);
        _stores.TryAdd(storeConfig.Alias, storeConfig).Assert(x => x == true, $"Could not add config={storeConfig}");
        return this;
    }

    public IFileStore Get(string alias)
    {
        alias.NotEmpty();
        _context.LogInformation("Gettig alias={alias}", alias);

        var fileStore = _cache.GetOrAdd(alias, _ =>
        {
            _stores.TryGetValue(alias, out StoreConfig? storeConfig).Assert(x => x == true, $"Alias={alias} not found");

            var createFileStore = storeConfig!.Create(_serviceProvider, storeConfig);
            return createFileStore;
        });

        return fileStore;
    }

    public (string alias, string filePath) GetAliasAndPath(string sourcePath, string? extension = null)
    {
        string[] parts = sourcePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .ToArray();

        (string alias, string path) result = parts switch
        {
            { Length: >= 2 } => (parts[0], setExtensionIfRequired(parts.Join('/')).ToLower()),
            _ => throw new ArgumentException($"Invalid grainId={sourcePath}"),
        };

        _context.LogInformation("Gettig alias and path for path={sourcePath}, extension={extension}, result={alias}, path={path}", sourcePath, extension, result.alias, result.path);
        return result;

        string setExtensionIfRequired(string path) => extension.IsEmpty() ? path : PathTool.SetExtension(path, extension);
    }
}
