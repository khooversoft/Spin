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

    public DataClientFactory(IServiceProvider serviceProvider, ILogger<DataClientFactory> logger)
    {
        _serviceProvider = serviceProvider.NotNull();
        _logger = logger.NotNull();
    }

    public IDataClient Create(string name)
    {
        name.NotEmpty();
        DataPipelineBuilder builder = _serviceProvider.GetRequiredKeyedService<DataPipelineBuilder>(name);

        var handler = builder.Handlers.BuildHandlers(_serviceProvider);

        IDataClient cache = handler switch
        {
            { StatusCode: StatusCode.OK } v => ActivatorUtilities.CreateInstance<DataClient>(_serviceProvider, v.Return()),
            _ => throw new ArgumentException("No handler specified"),
        };

        return cache;
    }

    public IDataClient<T> Create<T>()
    {
        string name = typeof(T).Name;
        DataPipelineBuilder builder = _serviceProvider.GetRequiredKeyedService<DataPipelineBuilder>(name);

        var handler = builder.Handlers.BuildHandlers(_serviceProvider);

        IDataClient<T> cache = handler switch
        {
            { StatusCode: StatusCode.OK } v => ActivatorUtilities.CreateInstance<DataClient<T>>(_serviceProvider, v.Return()),
            _ => throw new ArgumentException("No handler specified"),
        };

        return cache;
    }
}
