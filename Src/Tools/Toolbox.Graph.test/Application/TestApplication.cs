//using Toolbox.Test.Application;
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
}
