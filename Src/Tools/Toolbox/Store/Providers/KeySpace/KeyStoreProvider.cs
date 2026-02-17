using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace Toolbox.Store;

public class KeyStoreProvider : IStoreKeyProvider
{
    private readonly ILogger<KeyStoreProvider> _logger;
    private readonly IServiceProvider _serviceProvider;

    public KeyStoreProvider(string name, IServiceProvider serviceProvider, ILogger<KeyStoreProvider> logger)
    {
        Name = name.NotEmpty();
        _serviceProvider = serviceProvider.NotNull();
        _logger = logger.NotNull();
    }

    public string Name { get; }

    public IKeyStore GetStore(SpaceDefinition definition)
    {
        IKeyPathStrategy keySystem = GetStoreStrategy(definition);
        var store = ActivatorUtilities.CreateInstance<KeySpace>(_serviceProvider, keySystem);
        return store;
    }

    public IKeyStore<T> GetStore<T>(SpaceDefinition definition)
    {
        var baseStore = GetStore(definition);
        var store = ActivatorUtilities.CreateInstance<KeySpace<T>>(_serviceProvider, baseStore);
        return store;
    }

    private IKeyPathStrategy GetStoreStrategy(SpaceDefinition definition) => definition.SpaceFormat switch
    {
        SpaceFormat.Key => new KeyPathStrategy(definition.BasePath, definition.UseCache),
        SpaceFormat.Hash => new KeyHashStrategy(definition.BasePath, definition.UseCache),
        _ => throw new Exception($"Unsupported space format {definition.SpaceFormat} for key system"),
    };
}
