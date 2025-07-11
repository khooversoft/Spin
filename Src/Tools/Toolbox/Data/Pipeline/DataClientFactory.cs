using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class DataClientFactory
{
    private readonly IServiceProvider _serviceProvider;
    public DataClientFactory(IServiceProvider serviceProvider, ILogger<DataClientFactory> logger) => _serviceProvider = serviceProvider.NotNull();


    public IDataClient<T> Create<T>(string pipelineName)
    {
        pipelineName = pipelineName.NotEmpty();

        DataPipelineConfig builder = _serviceProvider.GetDataPipelineBuilder<T>(pipelineName);

        var handler = builder.Handlers.BuildHandlers(_serviceProvider);

        IDataClient<T> client = handler switch
        {
            { StatusCode: StatusCode.OK } v => ActivatorUtilities.CreateInstance<DataClient<T>>(_serviceProvider, builder, v.Return()),
            _ => throw new ArgumentException("No handler specified"),
        };

        return client;
    }
}
