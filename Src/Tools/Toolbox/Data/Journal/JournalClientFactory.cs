using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace Toolbox.Data;

public class JournalClientFactory
{
    private readonly IServiceProvider _serviceProvider;
    public JournalClientFactory(IServiceProvider serviceProvider, ILogger<JournalClientFactory> logger) => _serviceProvider = serviceProvider.NotNull();


    public IJournalClient Create(string pipelineName)
    {
        pipelineName = pipelineName.NotEmpty();

        IDataClient<JournalEntry> dataClient = _serviceProvider.GetDataClient<JournalEntry>(pipelineName);

        var client = ActivatorUtilities.CreateInstance<JournalClient>(_serviceProvider, pipelineName, dataClient);
        return client;
    }
}
