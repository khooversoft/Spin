//using Toolbox.Test.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Azure;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Application;

public static class TestApplication
{
    public static async Task<GraphHostService> CreateTestGraphService(
        GraphMap? graphMap = null,
        ITestOutputHelper? logOutput = null,
        bool disableCache = false,
        bool shareMode = false,
        string? logPrefix = null
        )
    {
        logPrefix = logPrefix.IsNotEmpty() ? $"[{logPrefix}] " : "[] ";

        GraphHostService graphHostService = await new GraphHostBuilder()
            .SetMap(graphMap)
            .UseInMemoryStore(true)
            .SetShareMode(shareMode)
            .UseLogging(true)
            .AddLogFilter("Toolbox.Graph.GraphQueryExecute", LogLevel.Warning)
            .SetLogOutput(x => logOutput?.WriteLine(logPrefix + x))
            .SetDisableCache(disableCache)
            .BuildAndRun();

        return graphHostService;
    }

    public static async Task<GraphHostService> CreateTestGraphServiceWithDatalake(
        string basePath,
        GraphMap? graphMap = null,
        Action<string>? logOutput = null,
        bool disableCache = false,
        bool shareMode = false
        )
    {
        DatalakeOption option = ReadOption(basePath.NotEmpty());

        GraphHostService graphHostService = await new GraphHostBuilder()
            .SetMap(graphMap)
            .SetShareMode(true)
            .UseLogging(true)
            .AddLogFilter("Toolbox.Graph.GraphQueryExecute", LogLevel.Warning)
            .AddLogFilter("Toolbox.Graph.GraphMapStore", LogLevel.Trace)
            .AddLogFilter("Toolbox.Graph.GraphLeaseControl", LogLevel.Trace)
            //.SetLogLevel(LogLevel.Trace)
            .SetLogOutput(logOutput)
            .AddDatalakeFileStore(option)
            .SetDisableCache(disableCache)
            .BuildAndRun();

        return graphHostService;
    }

    public static async Task<(GraphHostService testClient, ScopeContext context)> CreateInMemory<T>(ITestOutputHelper output)
    {
        var testClient = await new GraphHostBuilder()
            .UseInMemoryStore()
            .SetShareMode(true)
            .UseLogging()
            .AddLogFilter("Toolbox.Graph.GraphQueryExecute", LogLevel.Warning)
            .SetLogOutput(x => output.WriteLine(x))
            .SetDisableCache(true)
            .BuildAndRun();

        var context = testClient.CreateScopeContext<T>();
        return (testClient, context);
    }

    public static (IHost Host, ScopeContext Context) CreateDatalakeDirect<T>(string basePath, ITestOutputHelper output)
    {
        DatalakeOption datalakeOption = ReadOption(basePath);

        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services
                    .AddDatalakeFileStore(datalakeOption)
                    .AddLogging(config =>
                    {
                        config.AddDebug();
                        config.AddConsole();
                        config.AddFilter(x => true);
                        config.AddLambda(x => output.WriteLine(x));
                    });
            })
            .Build();

        ScopeContext context = new ScopeContext(host.Services.GetRequiredService<ILogger<T>>());
        return (host, context);
    }

    public static async Task<(GraphHostService testClient, ScopeContext context)> CreateDatalake<T>(string basePath, ITestOutputHelper output, bool noClear = false)
    {
        DatalakeOption datalakeOption = ReadOption(basePath);

        var testClient = await new GraphHostBuilder()
            .UseLogging()
            .SetShareMode(false)
            .AddLogFilter("Toolbox.Graph.GraphQueryExecute", LogLevel.Warning)
            .AddLogFilter("Toolbox.Graph.GraphMapStore", LogLevel.Trace)
            .AddLogFilter("Toolbox.Graph.GraphLeaseControl", LogLevel.Trace)
            .SetLogOutput(x => output.WriteLine(x))
            .SetDisableCache(true)
            .AddDatalakeFileStore(datalakeOption)
            .BuildAndRun();

        var context = testClient.CreateScopeContext<T>();

        if (!noClear) (await testClient.Execute("delete (*) ;", context)).BeOk();

        return (testClient, context);
    }

    public static async Task<(GraphHostService client1, GraphHostService client2, ScopeContext context)> CreateTwoLinkedForInMemory<T>(ITestOutputHelper output)
    {
        var firstClient = await create(null, x => output.WriteLine($"1st: {x}"));

        MemoryStore memoryStore = firstClient.Services.GetRequiredService<MemoryStore>();
        var secondClient = await create(memoryStore, x => output.WriteLine($"2nd: {x}"));

        var context = firstClient.CreateScopeContext<T>();
        (await firstClient.Execute("delete (*) ;", context)).IsOk().BeTrue();
        return (firstClient, secondClient, context);

        async Task<GraphHostService> create(MemoryStore? memoryStore, Action<string> outputFunc)
        {
            var result = await new GraphHostBuilder()
                .UseInMemoryStore(memoryStore == null)
                .SetShareMode(true)
                .UseLogging()
                .AddLogFilter("Toolbox.Graph.GraphQueryExecute", LogLevel.Warning)
                .SetLogOutput(outputFunc)
                .SetDisableCache(true)
                .AddServiceConfiguration(x => x.AddInMemoryFileStore(memoryStore))
                .BuildAndRun();

            return result;
        }
    }

    public static async Task<(GraphHostService client1, GraphHostService client2, ScopeContext context)> CreateTwoLinkClientsForDatalake<T>(string basePath, ITestOutputHelper output)
    {
        var firstClient = await TestApplication.CreateTestGraphServiceWithDatalake(basePath, logOutput: x => output.WriteLine($"1st: {x}"), shareMode: true);
        var secondClient = await TestApplication.CreateTestGraphServiceWithDatalake(basePath, logOutput: x => output.WriteLine($"2nd: {x}"), shareMode: true);

        var context = firstClient.CreateScopeContext<T>();
        (await firstClient.Execute("delete (*) ;", context)).IsOk().BeTrue();

        return (firstClient, secondClient, context);
    }

    private static DatalakeOption ReadOption(string basePath) => new ConfigurationBuilder()
        .AddJsonFile("application/TestSettings.json")
        .AddUserSecrets("Toolbox-Azure-test")
        .Build()
        .Get<DatalakeOption>().NotNull()
        .Func(x => x with { BasePath = basePath });
}
