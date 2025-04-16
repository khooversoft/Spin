using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Journal;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class GraphStartup
{
    public static IServiceCollection AddGraphEngine(this IServiceCollection services, GraphHostOption? hostOption = null)
    {
        services.NotNull();
        hostOption ??= new GraphHostOption();

        services.AddSingleton<GraphHostOption>(hostOption);
        services.AddSingleton<IGraphHost, GraphHost>();
        services.AddSingleton<IGraphEngine, GraphEngine>();
        services.AddSingleton<GraphMapStore>();
        services.AddSingleton<GraphLeaseControl>();
        services.AddSingleton<GraphMapCounter>();
        services.AddSingleton<IGraphMapFactory, GraphMapFactory>();

        services.AddJournalLog(GraphConstants.TrxJournal.DiKeyed, new JournalFileOption
        {
            ConnectionString = GraphConstants.TrxJournal.ConnectionString,
            ReadOnly = hostOption.ReadOnly,
        });

        services.AddSingleton<IGraphClient, GraphClientInMemory>();
        services.AddSingleton<IGraphStore, GraphStore>();

        if (!hostOption.DisableCache) services.TryAddSingleton<IMemoryCache, MemoryCache>();

        return services;
    }

    public static async Task<Option> StartGraphEngine(this IServiceProvider services)
    {
        var logger = services.GetRequiredService<IGraphHost>();
        var context = new ScopeContext(services.GetRequiredService<ILogger<GraphHost>>());

        var graphHost = services.GetRequiredService<IGraphHost>();
        var result = await graphHost.Run(context);
        if (result.IsError())
        {
            context.LogError("Failed to start graph host, result={result}", result);
            return result;
        }

        context.LogInformation("Graph host started");
        return StatusCode.OK;
    }
}

