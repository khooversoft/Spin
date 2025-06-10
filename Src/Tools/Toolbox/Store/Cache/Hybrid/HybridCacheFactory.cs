using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Toolbox.Tools;
using Toolbox.Types;

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

        var handler = GetHandlers(builder);

        IHybridCache cache = handler switch
        {
            { StatusCode: StatusCode.OK } v => v.Return(),
            _ => ActivatorUtilities.CreateInstance<HybridCacheDefault>(_serviceProvider),
        };

        return cache;
    }

    public IHybridCache<T> Create<T>()
    {
        string name = typeof(T).Name;
        HybridCacheBuilder builder = _serviceProvider.GetRequiredKeyedService<HybridCacheBuilder>(name);

        var handler = GetHandlers(builder);

        IHybridCache<T> cache = handler switch
        {
            { StatusCode: StatusCode.OK } v => ActivatorUtilities.CreateInstance<HybridCache<T>>(_serviceProvider, v.Return()),
            _ => ActivatorUtilities.CreateInstance<HybridCacheDefault<T>>(_serviceProvider),
        };

        return cache;
    }

    private Option<HybridCacheHandler> GetHandlers(HybridCacheBuilder builder)
    {
        var result = builder.Handlers
            .Reverse()
            .Aggregate((HybridCacheHandler?)null, (prev, current) =>
            {
                var handler = current(_serviceProvider);
                handler.InnerHandler = prev;
                return handler;
            });

        return result switch
        {
            null => StatusCode.NotFound,
            _ => result,
        };
    }
}
