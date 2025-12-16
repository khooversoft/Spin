using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace Toolbox.Store;

public class ListStoreProvider : IStoreListProvider
{
    private readonly ILogger<KeyStoreProvider> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ListStoreProvider(string name, IServiceProvider serviceProvider, ILogger<KeyStoreProvider> logger)
    {
        Name = name.NotEmpty();
        _serviceProvider = serviceProvider.NotNull();
        _logger = logger.NotNull();
    }

    public string Name { get; }

    public IListStore2<T> GetStore<T>(SpaceDefinition definition)
    {
        definition.SpaceFormat.Assert(x => x == SpaceFormat.List, $"Invalid space format {definition.SpaceFormat} for list store");

        ListKeySystem<T> listKeySystem = new(definition.BasePath);
        var store = ActivatorUtilities.CreateInstance<ListSpace<T>>(_serviceProvider, listKeySystem);

        return store;
    }
}
