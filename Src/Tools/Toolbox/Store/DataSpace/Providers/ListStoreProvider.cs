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

public class ListStoreProvider : IStoreListProvider
{
    private interface IRegistration { }
    private record StoreRegistration<T>(string Name, SpaceDefinition SpaceDefinition, SpaceSerializer? serializer, IListStore2<T> Store) : IRegistration;

    private readonly ILogger<KeyStoreProvider> _logger;
    private readonly ConcurrentDictionary<string, IRegistration> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly IServiceProvider _serviceProvider;

    public ListStoreProvider(string name, IServiceProvider serviceProvider, ILogger<KeyStoreProvider> logger)
    {
        Name = name.NotEmpty();
        _serviceProvider = serviceProvider.NotNull();
        _logger = logger.NotNull();
    }

    public string Name { get; }

    public IListStore2<T> GetStore<T>(SpaceDefinition definition, SpaceSerializer? serializer)
    {
        definition.SpaceFormat.Assert(x => x == SpaceFormat.List, $"Invalid space format {definition.SpaceFormat} for list store");

        IRegistration registration = _cache.GetOrAdd(definition.Name, _ =>
        {
            ListKeySystem<T> listKeySystem = new(definition.BasePath, serializer);
            var store = ActivatorUtilities.CreateInstance<ListSpace<T>>(_serviceProvider, listKeySystem);
            return new StoreRegistration<T>(definition.Name, definition, serializer, store);
        });

        IListStore2<T> store = registration is StoreRegistration<T> storeReg
            ? storeReg.Store
            : throw new InvalidOperationException($"Store type missmatch for registration {definition.Name}, expected {typeof(T).FullName}");

        storeReg.SpaceDefinition.Be(definition, "Registration definition missmatch, space in registry is not what is requested");
        return store;
    }
}
