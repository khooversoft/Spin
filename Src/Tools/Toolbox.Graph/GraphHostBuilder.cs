using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphHostBuilder
{
    private readonly ConcurrentQueue<Action<IServiceCollection>> _services = new();

    public GraphHostBuilder() { }

    public GraphMap? GraphMap { get; set; }
    public bool ReadOnly { get; set; }
    public bool ShareMode { get; set; }
    public bool DisableCache { get; set; }
    public bool InMemoryStore { get; set; }
    public bool Logging { get; set; }
    public string? ConfigurationFile { get; set; }
    public Action<string>? LogOutput { get; set; }

    public GraphHostBuilder SetMap(GraphMap? map) => this.Action(x => x.GraphMap = map);
    public GraphHostBuilder SetReadOnly(bool readOnly = true) => this.Action((Action<GraphHostBuilder>)(x => x.ReadOnly = readOnly));
    public GraphHostBuilder SetShareMode(bool shareMode = true) => this.Action(x => x.ReadOnly = shareMode);
    public GraphHostBuilder SetDisableCache(bool disableCache = true) => this.Action(x => x.DisableCache = disableCache);
    public GraphHostBuilder UseInMemoryStore(bool use = true) => this.Action(x => x.InMemoryStore = use);
    public GraphHostBuilder UseLogging(bool useLogging = true) => this.Action(x => x.Logging = useLogging);
    public GraphHostBuilder SetConfigurationFile(string? configurationFile) => this.Action(x => x.ConfigurationFile = configurationFile);
    public GraphHostBuilder AddServiceConfiguration(Action<IServiceCollection>? config) => this.Action(x => config.IfNotNull(config => x._services.Enqueue(config)));
    public GraphHostBuilder SetLogOutput(Action<string>? logOutput) => this.Action(x => x.LogOutput = logOutput);


    public async Task<GraphHostService> Build()
    {
        var option = new GraphHostOption
        {
            ReadOnly = ReadOnly,
            ShareMode = ShareMode,
            DisableCache = DisableCache,
        };

        ServiceProvider services = new ServiceCollection()
            .AddGraphEngine(option)
            .Action(x => ConfigurationFile?.Action(y => x.AddSingleton(ReadConfiguration())))
            .IfTrue(x => InMemoryStore, x => x.AddInMemoryFileStore())
            .IfTrue(x => Logging, x => x.AddLogging(config => config.AddDebug().AddConsole()))
            .IfTrue(x => LogOutput != null, x => x.AddLogging(config => config.AddLambda(LogOutput.NotNull())))
            .Action(x => _services.ForEach(y => y.Invoke(x)))
            .BuildServiceProvider();

        var graphEngine = new GraphHostService(services);
        ScopeContext context = graphEngine.CreateScopeContext<GraphHostService>();
        IGraphHost graphHost = services.GetRequiredService<IGraphHost>();

        var runOption = GraphMap switch
        {
            null => await graphHost.Run(context).ConfigureAwait(false),
            GraphMap v => await graphHost.Run(v, context).ConfigureAwait(false),
        };

        return graphEngine;
    }

    private IConfiguration ReadConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(ConfigurationFile.NotEmpty())
            .Build();

        string? secretName = configuration["SecretName"];

        configuration = new ConfigurationBuilder()
            .AddJsonFile(ConfigurationFile)
            .Action(x => secretName?.Action(y => x.AddUserSecrets(y)))
            .Build();

        return configuration;
    }
}

public class GraphHostService : IGraphClient, IAsyncDisposable
{
    public GraphHostService(ServiceProvider services)
    {
        services.NotNull();

        GraphEngine = services.GetRequiredService<IGraphEngine>();
        Services = services.NotNull();
    }

    public ServiceProvider Services { get; }
    public IGraphEngine GraphEngine { get; }
    public GraphMap Map => GraphEngine.Map;

    public ValueTask DisposeAsync() => Services.DisposeAsync();

    public Task<Option<QueryResult>> Execute(string command, ScopeContext context) => GraphEngine.Execute(command, context);
    public Task<Option<QueryBatchResult>> ExecuteBatch(string command, ScopeContext context) => GraphEngine.ExecuteBatch(command, context);

    public ScopeContext CreateScopeContext<T>() => new ScopeContext(Services.GetRequiredService<ILogger<T>>());
}

