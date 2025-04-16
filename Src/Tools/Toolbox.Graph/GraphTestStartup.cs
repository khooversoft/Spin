using Microsoft.Extensions.DependencyInjection;

namespace Toolbox.Graph;

public static class GraphTestStartup
{
    public static async Task<GraphHostService> CreateGraphService(
        GraphMap? graphMap = null,
        Action<IServiceCollection>? config = null,
        Action<string>? logOutput = null,
        bool sharedMode = false,
        bool useInMemoryStore = true,
        bool disableCache = false
        )
    {
        GraphHostService graphHostService = await new GraphHostBuilder()
            .SetMap(graphMap)
            .UseInMemoryStore(useInMemoryStore)
            .SetShareMode(sharedMode)
            .UseLogging()
            .SetLogOutput(logOutput)
            .AddServiceConfiguration(config)
            .SetDisableCache(disableCache)
            .Build();

        return graphHostService;
    }
}

