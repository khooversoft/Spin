using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace Toolbox.Store;

public class KeyStoreProvider : IStoreKeyProvider
{
    private record StoreRegistration(string Name, SpaceDefinition spaceDefinition, IKeyStore Store);

    private readonly ILogger<KeyStoreProvider> _logger;
    private readonly ConcurrentDictionary<string, StoreRegistration> _cache = new(StringComparer.OrdinalIgnoreCase);
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
        StoreRegistration registration = _cache.GetOrAdd(definition.Name, _ =>
        {
            var store = createStore(definition.SpaceFormat);
            return new StoreRegistration(definition.Name, definition, store);
        });

        registration.spaceDefinition.Be(definition, "Registration definition missmatch, space in registry is not what is requested");
        return registration.Store;

        IKeyStore createStore(SpaceFormat format)
        {
            var keySystem = createKeySystem(format);
            // TODO: KeySystem
            return ActivatorUtilities.CreateInstance<KeySpace>(_serviceProvider, keySystem);
        }

        IKeySystem createKeySystem(SpaceFormat format) => format switch
        {
            SpaceFormat.Key => new KeySystem(definition.BasePath),
            SpaceFormat.Hash => new HashKeySystem(definition.BasePath),
            _ => throw new Exception($"Unsupported space format {definition.SpaceFormat} for key system"),
        };
    }
}
