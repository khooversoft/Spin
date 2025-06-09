using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Toolbox.Tools;

namespace Toolbox.Store;

public class HybridCacheFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HybridCacheFactory> _logger;

    public HybridCacheFactory(IServiceProvider serviceProvider, IOptionsMonitor<HybridCacheBuilder> optionMonitor, ILogger<HybridCacheFactory> logger)
    {
        _serviceProvider = serviceProvider.NotNull();
        _logger = logger.NotNull();
    }

    public IHybridCache Create(string name)
    {
        name.NotEmpty();
        HybridCacheBuilder builder = _serviceProvider.GetRequiredKeyedService<HybridCacheBuilder>(name);

        var handlers = GetHandlers(builder);

        if (!handlers.Any()) return ActivatorUtilities.CreateInstance<HybridCacheDefault>(_serviceProvider);
        return handlers.First();
    }

    public IHybridCache<T> Create<T>()
    {
        string name = typeof(T).Name;
        HybridCacheBuilder builder = _serviceProvider.GetRequiredKeyedService<HybridCacheBuilder>(name);

        var handlers = GetHandlers(builder);

        IHybridCache<T> handler = handlers.Any() switch
        {
            true => ActivatorUtilities.CreateInstance<HybridCache<T>>(_serviceProvider, handlers.First()),
            false => ActivatorUtilities.CreateInstance<HybridCacheDefault<T>>(_serviceProvider)
        };

        return handler;
    }

    private IReadOnlyList<HybridCacheHandler> GetHandlers(HybridCacheBuilder builder)
    {
        HybridCacheHandler? lastHandler = null;

        var result = builder.Handlers
            .Reverse()
            .Select(x =>
            {
                var handler = x(_serviceProvider);
                handler.InnerHandler = lastHandler;
                lastHandler = handler;
                return handler;
            })
            .Reverse()
            .ToArray();

        return result;
    }
}
