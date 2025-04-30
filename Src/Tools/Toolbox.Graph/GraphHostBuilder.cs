using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphHostBuilder
{
    private readonly ConcurrentQueue<Action<IServiceCollection>> _services = new();
    private readonly ConcurrentDictionary<string, LogLevel> _logLevels = new(StringComparer.OrdinalIgnoreCase);

    public GraphHostBuilder() { }

    public GraphMap? GraphMap { get; set; }
    public bool ReadOnly { get; set; }
    public bool ShareMode { get; set; }
    public bool DisableCache { get; set; }
    public bool InMemoryStore { get; set; }
    public bool Logging { get; set; }
    public LogLevel LogLevel { get; set; } = LogLevel.None;
    public string? ConfigurationFile { get; set; }
    public Action<string>? LogOutput { get; set; }

    public GraphHostBuilder SetMap(GraphMap? map) => this.Action(x => x.GraphMap = map);
    public GraphHostBuilder SetReadOnly(bool readOnly = true) => this.Action((Action<GraphHostBuilder>)(x => x.ReadOnly = readOnly));
    public GraphHostBuilder SetShareMode(bool shareMode = true) => this.Action(x => x.ShareMode = shareMode);
    public GraphHostBuilder SetDisableCache(bool disableCache = true) => this.Action(x => x.DisableCache = disableCache);
    public GraphHostBuilder UseInMemoryStore(bool use = true) => this.Action(x => x.InMemoryStore = use);
    public GraphHostBuilder UseLogging(bool useLogging = true) => this.Action(x => x.Logging = useLogging);
    public GraphHostBuilder SetLogLevel(LogLevel useLogging = LogLevel.None) => this.Action(x => x.LogLevel = useLogging);
    public GraphHostBuilder AddLogFilter(string name, LogLevel level) => this.Action(x => x._logLevels.AddOrUpdate(name, level, (key, oldValue) => level));
    public GraphHostBuilder SetConfigurationFile(string? configurationFile) => this.Action(x => x.ConfigurationFile = configurationFile);
    public GraphHostBuilder SetLogOutput(Action<string>? logOutput) => this.Action(x => x.LogOutput = logOutput);

    public GraphHostBuilder AddServiceConfiguration(Action<IServiceCollection>? config)
    {
        if (config != null) _services.Enqueue(config);
        return this;
    }

    public async Task<GraphHostService> Build()
    {
        var option = new GraphHostOption
        {
            ReadOnly = ReadOnly,
            ShareMode = ShareMode,
            DisableCache = DisableCache,
        };

        LogLevel logLevel = LogLevel;
        bool showLog(LogLevel level) => (int)level >= (int)logLevel;

        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services
                    .AddGraphEngine(option)
                    .AddSingleton<GraphHostService>()
                    .Action(x => ConfigurationFile?.Action(y => x.AddSingleton(ReadConfiguration())))
                    .IfTrue(x => InMemoryStore, x => x.AddInMemoryFileStore())
                    .IfTrue(x => Logging, x => x.AddLogging(config =>
                    {
                        config.AddDebug();
                        config.AddConsole();
                        config.AddFilter(showLog);
                        _logLevels.ForEach(y => config.AddFilter(y.Key, y.Value));
                    }))
                    .IfTrue(x => LogOutput != null, x => x.AddLogging(config => config.AddLambda(LogOutput.NotNull())))
                    .Action(x => _services.ForEach(y => y.Invoke(x)));
            })
            .Build();

        var graphEngine = host.Services.GetRequiredService<GraphHostService>();
        ScopeContext context = graphEngine.CreateScopeContext<GraphHostService>();
        IGraphHost graphHost = graphEngine.Services.GetRequiredService<IGraphHost>();

        var runOption = GraphMap switch
        {
            null => await graphHost.Run(context).ConfigureAwait(false),
            GraphMap v => await graphHost.Run(v, context).ConfigureAwait(false),
        };

        runOption.LogStatus(context, "Run engine").ThrowOnError();
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

