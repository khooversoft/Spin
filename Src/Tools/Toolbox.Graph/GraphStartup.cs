using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Store;
using Toolbox.Tools;

namespace Toolbox.Graph;

public static class GraphStartup
{
    public static IServiceCollection AddGraphTrace(this IServiceCollection services)
    {
        services.NotNull();
        services.AddSingleton<IChangeTrace, InMemoryChangeTrace>();
        services.AddSingleton<IGraphFileStore, FileStoreTraceShim>();

        return services;
    }

    public static IServiceCollection AddGraphFileStore(this IServiceCollection services)
    {
        services.NotNull().AddSingleton(services => (IGraphFileStore)services.GetRequiredService<IFileStore>());

        return services;
    }

    public static IServiceCollection AddGraphEngine(this IServiceCollection services)
    {
        services.NotNull();
        services.AddSingleton<GraphMap>();
        services.AddSingleton<IGraphContext, GraphContext>();
        services.AddSingleton<IGraphClient, GraphClient>();

        return services;
    }
}

public static class GraphTestStartup
{
    public static IServiceCollection AddGraphTestHost(this IServiceCollection services)
    {
        services.NotNull();

        services.AddInMemoryFileStore();
        services.AddGraphTrace();
        services.AddGraphFileStore();
        services.AddGraphEngine();

        return services;
    }

    public static GraphTestClient CreateGraphTestHost()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddGraphTestHost()
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
