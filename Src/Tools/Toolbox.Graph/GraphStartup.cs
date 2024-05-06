﻿using Microsoft.Extensions.DependencyInjection;
using Toolbox.Tools;

namespace Toolbox.Graph;

public static class GraphStartup
{
    //public static IServiceCollection AddGraphFileStore(this IServiceCollection services)
    //{
    //    services.NotNull().AddSingleton(services => (IGraphFileStore)services.GetRequiredService<IFileStore>());

    //    return services;
    //}

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
            .AddSingleton<IGraphClient, GraphClient>()
            .BuildServiceProvider();

        var graphClient = new GraphTestClient(services.GetRequiredService<IGraphContext>(), services);
        return graphClient;
    }
}

public class GraphTestClient : GraphClient, IGraphClient
{
    public GraphTestClient(IGraphContext graphContext, IServiceProvider serviceProvider)
        : base(graphContext)
    {
        ServiceProvider = serviceProvider.NotNull();
    }

    public IServiceProvider ServiceProvider { get; }
}
