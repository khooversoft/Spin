using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Toolbox.Journal;
using Toolbox.Store;
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
        services.AddSingleton<IGraphEngine, GraphEngine>();
        services.AddSingleton<GraphMapCounter>();
        services.AddSingleton<IGraphMapFactory, GraphMapFactory>();

        services.AddJournalLog(GraphConstants.TrxJournal.DiKeyed, new JournalFileOption
        {
            ConnectionString = GraphConstants.TrxJournal.ConnectionString,
            UseBackgroundWriter = hostOption.UseBackgroundWriter,
        });

        services.AddSingleton<IGraphClient, GraphQueryExecute>();
        services.AddSingleton<IGraphFileStore, GraphFileStore>();

        if (hostOption.ShareMode)
            services.AddSingleton<IGraphMapAccess, MapSharedAccess>();
        else
            services.AddSingleton<IGraphMapAccess, MapExclusiveAccess>();

        if (!hostOption.DisableCache)
        {
            services.TryAddSingleton<IMemoryCache, MemoryCache>();
            services.TryAddSingleton<MemoryCacheAccess>();
        }

        return services;
    }

    public static async Task<Option> StartGraphEngine(this IServiceProvider serviceProvider, GraphMap? map = null)
    {
        serviceProvider.NotNull();
        var graphHost = serviceProvider.GetRequiredService<IGraphEngine>();
        var context = serviceProvider.GetRequiredService<ILogger<GraphEngine>>().ToScopeContext();

        var result = map switch
        {
            null => await graphHost.Start(context).ConfigureAwait(false),
            _ => await graphHost.Start(map, context).ConfigureAwait(false),
        };

        return result;
    }
}

