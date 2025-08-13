using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public record ListStoreBuilder<T>
{
    private readonly IList<Func<IServiceProvider, IListStoreProvider<T>>> _handlers = new List<Func<IServiceProvider, IListStoreProvider<T>>>();

    public ListStoreBuilder(IServiceCollection services) => Services = services;

    public string? BasePath { get; set; } = null!;
    public IServiceCollection Services { get; }

    public void Add(Func<IServiceProvider, IListStoreProvider<T>> handler) => _handlers.Add(handler.NotNull());
    public void Add<P>() where P : class, IListStoreProvider<T> => _handlers.Add(service => service.GetRequiredService<P>());

    public Option<IListStoreProvider<T>> BuildHandlers(IServiceProvider serviceProvider, IListStore<T> storeProvider)
    {
        IListStoreProvider<T>? firstHandler = _handlers
            .Reverse()
            .Aggregate((IListStoreProvider<T>?)null, (prev, current) =>
            {
                var handler = current(serviceProvider);
                handler.InnerHandler = prev ?? storeProvider;
                return handler;
            });

        return firstHandler switch
        {
            null => StatusCode.NotFound,
            _ => firstHandler.Cast<IListStoreProvider<T>>().ToOption(),
        };
    }
}
