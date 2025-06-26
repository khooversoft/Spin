using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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


    public IDataClient<T> Create<T>(string pipelineName)
    {
        pipelineName = pipelineName.NotEmpty();

        DataPipelineBuilder builder = _serviceProvider.GetDataPipelineBuilder<T>(pipelineName);

        var handler = builder.Handlers.BuildHandlers(_serviceProvider);

        IDataClient<T> cache = handler switch
        {
            { StatusCode: StatusCode.OK } v => ActivatorUtilities.CreateInstance<DataClient<T>>(_serviceProvider, builder, v.Return()),
            _ => throw new ArgumentException("No handler specified"),
        };

        return cache;
    }
}
