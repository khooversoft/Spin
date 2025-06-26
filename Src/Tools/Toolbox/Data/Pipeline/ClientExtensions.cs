using Microsoft.Extensions.DependencyInjection;
using Toolbox.Tools;

namespace Toolbox.Data;

public static class ClientExtensions
{
    public static IDataClient<T> GetDataClient<T>(this IServiceProvider serviceProvider, string pipelineName)
    {
        serviceProvider.NotNull();
        var factory = serviceProvider.GetRequiredService<DataClientFactory>();
        return factory.Create<T>(pipelineName);
    }

    public static IJournalClient<T> GetJournalClient<T>(this IServiceProvider serviceProvider, string pipelineName)
    {
        serviceProvider.NotNull();
        var factory = serviceProvider.GetRequiredService<JournalClientFactory>();
        return factory.Create<T>(pipelineName);
    }
}
