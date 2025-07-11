using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class GraphTestStartup
{
    private record GraphTestStartupLogger() { }

    public static async Task<GraphHostService> CreateGraphService(
        GraphMap? graphMap = null,
        Action<IServiceCollection>? config = null,
        Action<string>? logOutput = null,
        bool sharedMode = false,
        bool useInMemoryStore = true,
        bool disableCache = false
        )
    {
        var builder = CreateBuilder(graphMap, config, logOutput, sharedMode, useInMemoryStore, disableCache);
        GraphHostService graphHostService = builder.Build();
        var context = graphHostService.Services.GetRequiredService<ILogger<GraphTestStartupLogger>>().ToScopeContext();

        var startResult = await graphHostService.Services.StartGraphEngine(graphMap);
        if (startResult.IsError()) startResult.LogStatus(context, "Graph engine start").ThrowOnError();
        return graphHostService;
    }

    public static GraphHostBuilder CreateBuilder(GraphMap? graphMap = null,
        Action<IServiceCollection>? config = null,
        Action<string>? logOutput = null,
        bool sharedMode = false,
        bool useInMemoryStore = true,
        bool disableCache = false
        )
    {
        var builder = new GraphHostBuilder()
            .SetMap(graphMap)
            .UseInMemoryStore(useInMemoryStore)
            .SetShareMode(sharedMode)
            .UseLogging()
            .SetLogOutput(logOutput)
            .AddServiceConfiguration(config)
            .SetDisableCache(disableCache);

        return builder;
    }
}

