using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Journal;
using Toolbox.Store;
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
        services.AddSingleton<GraphMapCounter>();
        services.AddSingleton<IGraphMapFactory, GraphMapFactory>();

        services.AddJournalLog(GraphConstants.TrxJournal.DiKeyed, new JournalFileOption
        {
            ConnectionString = GraphConstants.TrxJournal.ConnectionString,
            ReadOnly = hostOption.ReadOnly,
        });

        services.AddSingleton<IGraphClient, GraphQueryRunner>();
        services.AddSingleton<IGraphStore, GraphStore>();

        if (!hostOption.DisableCache)
        {
            services.TryAddSingleton<IMemoryCache, MemoryCache>();
            services.TryAddSingleton<MemoryCacheAccess>();
        }

        return services;
    }
}

