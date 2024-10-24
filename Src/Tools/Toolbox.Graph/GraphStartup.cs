using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.TransactionLog;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class GraphStartup
{
    public static IServiceCollection AddGraphEngine(this IServiceCollection services)
    {
        services.NotNull();

        services.AddSingleton<IGraphHost, GraphHost>();
        services.AddTransactionLogProvider(GraphConstants.JournalConnectionString);
        services.TryAddSingleton<IGraphClient, GraphClientInMemory>();
        services.TryAddSingleton<IGraphStore, GraphFileStoreCache>();
        services.TryAddSingleton<IMemoryCache, MemoryCache>();

        return services;
    }
}

public static class GraphTestStartup
{
    public static GraphTestClient CreateGraphTestHost(GraphMap? graphMap = null, Action<IServiceCollection>? config = null)
    {
        var servicesCollection = new ServiceCollection()
            .AddLogging(config => config.AddDebug())
            .AddInMemoryFileStore()
            .AddGraphEngine();

        config?.Invoke(servicesCollection);
        var services = servicesCollection.BuildServiceProvider();

        IGraphHost graphContext = services.GetRequiredService<IGraphHost>();
        if (graphMap != null) graphContext.SetMap(graphMap);

        var graphClient = new GraphTestClient(graphContext, services);
        return graphClient;
    }
}

public class GraphTestClient : GraphClientInMemory, IGraphClient
{
    public GraphTestClient(IGraphHost graphContext, IServiceProvider serviceProvider)
        : base(graphContext)
    {
        ServiceProvider = serviceProvider.NotNull();
    }

    public IServiceProvider ServiceProvider { get; }

    public ScopeContext GetScopeContext<T>()
    {
        var logger = ServiceProvider.GetRequiredService<ILogger<T>>();
        return new ScopeContext(logger);
    }
}
