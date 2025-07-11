using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Tools;

namespace Toolbox.Data;

public static class JournalStart
{
    public static IJournalClient GetJournalClient(this IServiceProvider serviceProvider, string pipelineName)
    {
        serviceProvider.NotNull();
        var factory = serviceProvider.GetRequiredService<JournalClientFactory>();
        return factory.Create(pipelineName);
    }

    public static IServiceCollection AddJournalPipeline(this IServiceCollection services, string pipelineName, Action<DataPipelineConfig> config)
    {
        services.NotNull();
        config.NotNull();
        pipelineName.NotEmpty();

        services.AddDataPipeline<JournalEntry>(pipelineName, config);

        services.TryAddSingleton<JournalClientFactory>();
        services.TryAddSingleton<LogSequenceNumber>();

        return services;
    }
}
