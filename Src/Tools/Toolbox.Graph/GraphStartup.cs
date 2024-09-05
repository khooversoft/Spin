using Microsoft.Extensions.DependencyInjection;
using Toolbox.Tools;

namespace Toolbox.Graph;

public static class GraphStartup
{
    public static IServiceCollection AddGraphInMemoryFileStore(this IServiceCollection services)
    {
        services.NotNull().AddSingleton<IGraphFileStore, InMemoryGraphFileStore>();
        return services;
    }
}

public static class GraphTestStartup
{
    public static GraphTestClient CreateGraphTestHost(GraphMap? graphMap = null)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddGraphInMemoryFileStore()
            .AddSingleton<GraphMap>(graphMap ?? new GraphMap())
            .AddSingleton<IGraphContext, GraphContext>()
            .AddSingleton<IGraphClient, GraphClientInMemory>()
            .BuildServiceProvider();

        var graphClient = new GraphTestClient(services.GetRequiredService<IGraphContext>(), services);
        return graphClient;
    }

    public static GraphTestClient2 CreateGraphTestHost2(GraphMap? graphMap = null)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddGraphInMemoryFileStore()
            .AddSingleton<GraphMap>(graphMap ?? new GraphMap())
            .AddSingleton<IGraphContext, GraphContext>()
            .AddSingleton<IGraphClient2, GraphClientInMemory2>()
            .BuildServiceProvider();

        var graphClient = new GraphTestClient2(services.GetRequiredService<IGraphContext>(), services);
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

public class GraphTestClient2 : GraphClientInMemory2, IGraphClient2
{
    public GraphTestClient2(IGraphContext graphContext, IServiceProvider serviceProvider)
        : base(graphContext)
    {
        ServiceProvider = serviceProvider.NotNull();
    }

    public IServiceProvider ServiceProvider { get; }
}
