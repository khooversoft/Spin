using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public record KeyStoreBuilder<T>
{
    private readonly IList<Func<IServiceProvider, IKeyStore<T>>> _handlers = new List<Func<IServiceProvider, IKeyStore<T>>>();

    public KeyStoreBuilder(IServiceCollection services) => Services = services;

    public string? BasePath { get; set; } = null!;
    public IServiceCollection Services { get; }

    public void Add(Func<IServiceProvider, IKeyStore<T>> handler) => _handlers.Add(handler.NotNull());
    public void Add<P>() where P : class, IKeyStore<T> => _handlers.Add(service => service.GetRequiredService<P>());

    public Option<IKeyStore<T>> BuildHandlers(IServiceProvider serviceProvider, IKeyStore<T> storeProvider)
    {
        bool hasKeyStore = false;
        IKeyStore<T>? end = null;

        IKeyStore<T>? firstHandler = _handlers
            .Reverse()
            .Aggregate((IKeyStore<T>?)null, (prev, current) =>
            {
                var handler = current(serviceProvider);
                end ??= handler;

                if (handler is KeyStore<T>)
                {
                    if (hasKeyStore) throw new InvalidOperationException("Multiple KeyStore<T> instances found. Only one is allowed.");
                    hasKeyStore = true;
                }

                handler.InnerHandler = prev;
                return handler;
            });

        return firstHandler switch
        {
            null => StatusCode.NotFound,
            _ => hasKeyStore switch
            {
                true => firstHandler.Cast<IKeyStore<T>>().ToOption(),
                false => firstHandler.Action(_ => getEnd(firstHandler).InnerHandler = storeProvider).ToOption(),
            }
        };

        IKeyStore<T> getEnd(IKeyStore<T> node)
        {
            while (node.InnerHandler != null) node = node.InnerHandler;
            return node;
        }
    }
}
