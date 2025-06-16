using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class DataClientFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataClientFactory> _logger;

    public DataClientFactory(IServiceProvider serviceProvider, IOptionsMonitor<DataClientBuilder> optionMonitor, ILogger<DataClientFactory> logger)
    {
        _serviceProvider = serviceProvider.NotNull();
        _logger = logger.NotNull();
    }

    public IDataClient Create(string name)
    {
        name.NotEmpty();
        DataClientBuilder builder = _serviceProvider.GetRequiredKeyedService<DataClientBuilder>(name);

        var handler = GetHandlers(builder);

        IDataClient cache = handler switch
        {
            { StatusCode: StatusCode.OK } v => v.Return(),
            _ => ActivatorUtilities.CreateInstance<DataClientDefault>(_serviceProvider),
        };

        return cache;
    }

    public IDataClient<T> Create<T>()
    {
        string name = typeof(T).Name;
        DataClientBuilder builder = _serviceProvider.GetRequiredKeyedService<DataClientBuilder>(name);

        var handler = GetHandlers(builder);

        IDataClient<T> cache = handler switch
        {
            { StatusCode: StatusCode.OK } v => ActivatorUtilities.CreateInstance<DataClient<T>>(_serviceProvider, v.Return()),
            _ => ActivatorUtilities.CreateInstance<DataClientDefault<T>>(_serviceProvider),
        };

        return cache;
    }

    private Option<DataClientHandler> GetHandlers(DataClientBuilder builder)
    {
        var result = builder.Handlers
            .Reverse()
            .Aggregate((DataClientHandler?)null, (prev, current) =>
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
