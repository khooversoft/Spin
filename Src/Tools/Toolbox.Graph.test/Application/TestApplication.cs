//using Toolbox.Test.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Toolbox.Azure;
using Toolbox.Extensions;
using Toolbox.Tools;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Application;

public static class TestApplication
{
    public static async Task<GraphHostService> CreateTestGraphService(
        GraphMap? graphMap = null,
        ITestOutputHelper? logOutput = null,
        bool disableCache = false,
        bool shareMode = false
        )
    {
        GraphHostService graphHostService = await GraphTestStartup.CreateGraphService(
            graphMap: graphMap,
            logOutput: x => logOutput?.WriteLine(x),
            disableCache: disableCache,
            sharedMode: shareMode
        );

        return graphHostService;
    }

    public static async Task<GraphHostService> CreateTestGraphServiceWithDatalake(
        GraphMap? graphMap = null,
        Action<string>? logOutput = null,
        bool disableCache = false,
        bool shareMode = false
        )
    {
        DatalakeOption option = ReadOption("graphTesting");

        GraphHostService graphHostService = await new GraphHostBuilder()
            .SetMap(graphMap)
            .SetShareMode(true)
            .UseLogging(true)
            .AddLogFilter("Toolbox.Graph.GraphMapStore", LogLevel.Trace)
            .AddLogFilter("Toolbox.Graph.GraphLeaseControl", LogLevel.Trace)
            //.SetLogLevel(LogLevel.Trace)
            .SetLogOutput(logOutput)
            .AddDatalakeFileStore(option)
            .SetDisableCache(disableCache)
            .Build();

        return graphHostService;
    }

    private static DatalakeOption ReadOption(string basePath) => new ConfigurationBuilder()
        .AddJsonFile("application/TestSettings.json")
        .AddUserSecrets("Toolbox-Azure-test")
        .Build()
        .Get<DatalakeOption>().NotNull()
        .Func(x => x with { BasePath = basePath });
}
