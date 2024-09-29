using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.TransactionLog;

namespace Toolbox.Graph;

public static class GraphStartup
{
    public static IServiceCollection AddGraphEngine(this IServiceCollection services, string? mapDatabasePath = null)
    {
        services.NotNull();

        services.AddSingleton<IGraphContext, GraphContext>();
        services.AddTransactionLogProvider(GraphConstants.JournalConnectionString);
        services.AddSingleton<IGraphClient, GraphClientInMemory>();

        services.AddSingleton<IGraphMapStore, GraphMapStore>(s =>
        {
            var path = mapDatabasePath ?? GraphConstants.MapDatabasePath;
            var graphStore = ActivatorUtilities.CreateInstance<GraphMapStore>(s, path);
            return graphStore;
        });

        return services;
    }
}

public static class GraphTestStartup
{
    public static GraphTestClient CreateGraphTestHost(GraphMap? graphMap = null)
    {
        var services = new ServiceCollection()
            .AddLogging(config => config.AddDebug())
            .AddInMemoryFileStore()
            .AddGraphEngine()
            .BuildServiceProvider();

        IGraphContext graphContext = services.GetRequiredService<IGraphContext>();
        if (graphMap != null) graphContext.SetMap(graphMap);

        var graphClient = new GraphTestClient(graphContext, services);
        return graphClient;
    }
}

public class GraphTestClient : GraphClientInMemory, IGraphClient
{
    public GraphTestClient(IGraphContext graphContext, IServiceProvider serviceProvider)
        : base(graphContext)
    {
        ServiceProvider = serviceProvider.NotNull();
    }

    public IServiceProvider ServiceProvider { get; }
}
