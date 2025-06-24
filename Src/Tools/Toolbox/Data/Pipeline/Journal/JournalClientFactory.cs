using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class JournalClientFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataClientFactory> _logger;

    public JournalClientFactory(IServiceProvider serviceProvider, ILogger<DataClientFactory> logger)
    {
        _serviceProvider = serviceProvider.NotNull();
        _logger = logger.NotNull();
    }

    public IJournalClient<T> Create<T>()
    {
        string name = typeof(T).Name;
        DataPipelineBuilder builder = _serviceProvider.GetRequiredKeyedService<DataPipelineBuilder>(name);

        var handler = builder.Handlers.BuildHandlers(_serviceProvider);

        IJournalClient<T> cache = handler switch
        {
            { StatusCode: StatusCode.OK } v => ActivatorUtilities.CreateInstance<JournalClient<T>>(_serviceProvider, v.Return()),
            _ => throw new ArgumentException("No handler specified"),
        };

        return cache;
    }
}
