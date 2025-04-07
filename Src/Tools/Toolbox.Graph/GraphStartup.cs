using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Journal;
using Toolbox.Tools;

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
}

public static class GraphTestStartup
{
    public static async Task<GraphHostService> CreateGraphService(
        GraphMap? graphMap = null,
        Action<IServiceCollection>? config = null,
        Action<string>? logOutput = null,
        bool sharedMode = false
        )
    {
        GraphHostService graphHostService = await new GraphHostBuilder()
            .SetMap(graphMap)
            .UseInMemoryStore()
            .SetShareMode(sharedMode)
            .UseLogging()
            .SetLogOutput(logOutput)
            .AddServiceConfiguration(config)
            .Build();

        return graphHostService;
    }
}

