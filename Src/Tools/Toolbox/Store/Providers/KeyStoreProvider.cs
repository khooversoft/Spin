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
        IKeySystem keySystem = definition.SpaceFormat switch
        {
            SpaceFormat.Key => new KeySystem(definition.BasePath, definition.UseCache),
            SpaceFormat.Hash => new HashKeySystem(definition.BasePath),
            _ => throw new Exception($"Unsupported space format {definition.SpaceFormat} for key system"),
        };

        var store = ActivatorUtilities.CreateInstance<KeySpace>(_serviceProvider, keySystem);
        return store;
    }
}
