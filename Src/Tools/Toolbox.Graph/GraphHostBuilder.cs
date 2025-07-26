//using System.Collections.Concurrent;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph;

//public class GraphHostBuilder
//{
//    private readonly ConcurrentQueue<Action<IServiceCollection>> _services = new();
//    private readonly ConcurrentDictionary<string, LogLevel> _logLevels = new(StringComparer.OrdinalIgnoreCase);

//    public GraphHostBuilder() { }

//    public GraphMap? GraphMap { get; set; }
//    public bool ReadOnly { get; set; }
//    public bool ShareMode { get; set; }
//    public bool UseBackgroundWriter { get; set; }
//    public bool DisableCache { get; set; }
//    public bool InMemoryStore { get; set; }
//    public bool Logging { get; set; }
//    public LogLevel LogLevel { get; set; } = LogLevel.Information;
//    public string? ConfigurationFile { get; set; }
//    public Action<string>? LogOutput { get; set; }

//    public GraphHostBuilder SetMap(GraphMap? map) => this.Action(x => x.GraphMap = map);
//    public GraphHostBuilder SetReadOnly(bool readOnly = true) => this.Action((Action<GraphHostBuilder>)(x => x.ReadOnly = readOnly));
//    public GraphHostBuilder SetShareMode(bool shareMode = true) => this.Action(x => x.ShareMode = shareMode);
//    public GraphHostBuilder SetUseBackgroundWriter(bool breakExclusiveLease = true) => this.Action(x => x.UseBackgroundWriter = UseBackgroundWriter);
//    public GraphHostBuilder SetDisableCache(bool disableCache = true) => this.Action(x => x.DisableCache = disableCache);
//    public GraphHostBuilder UseInMemoryStore(bool use = true) => this.Action(x => x.InMemoryStore = use);
//    public GraphHostBuilder UseLogging(bool useLogging = true) => this.Action(x => x.Logging = useLogging);
//    public GraphHostBuilder SetLogLevel(LogLevel useLogging = LogLevel.None) => this.Action(x => x.LogLevel = useLogging);
//    public GraphHostBuilder AddLogFilter(string name, LogLevel level) => this.Action(x => x._logLevels.AddOrUpdate(name, level, (key, oldValue) => level));
//    public GraphHostBuilder SetConfigurationFile(string? configurationFile) => this.Action(x => x.ConfigurationFile = configurationFile);
//    public GraphHostBuilder SetLogOutput(Action<string>? logOutput) => this.Action(x => x.LogOutput = logOutput);

//    public GraphHostBuilder AddServiceConfiguration(Action<IServiceCollection>? config)
//    {
//        if (config != null) _services.Enqueue(config);
//        return this;
//    }

//    public GraphHostService Build()
//    {
//        IConfiguration? config = ConfigurationFile != null ? ReadConfiguration() : null;

//        GraphHostOption graphHostOption = config?.GetSection("GraphHost").Get<GraphHostOption>() ?? new GraphHostOption
//        {
//            ShareMode = ShareMode,
//            DisableCache = DisableCache,
//            UseBackgroundWriter = UseBackgroundWriter,
//        };

//        LogLevel logLevel = LogLevel;
//        bool showLog(LogLevel level) => (int)level >= (int)logLevel;

//        IHost host = Host.CreateDefaultBuilder()
//            .ConfigureServices((context, services) =>
//            {
//                services
//                    .AddGraphEngine(graphHostOption)
//                    .AddSingleton<GraphHostService>()
//                    .Action(x => config?.Action(y => x.AddSingleton(y)))
//                    .IfTrue(x => InMemoryStore, x => x.AddInMemoryFileStore())
//                    .IfTrue(x => Logging, x => x.AddLogging(config =>
//                    {
//                        config.AddDebug();
//                        config.AddConsole();
//                        config.AddFilter(showLog);
//                        _logLevels.ForEach(y => config.AddFilter(y.Key, y.Value));
//                    }))
//                    .IfTrue(x => LogOutput != null, x => x.AddLogging(config => config.AddLambda(LogOutput.NotNull())))
//                    .Action(x => _services.ForEach(y => y.Invoke(x)));
//            })
//            .Build();

//        var graphEngineService = host.Services.GetRequiredService<GraphHostService>();
//        return graphEngineService;
//    }

//    public async Task<GraphHostService> BuildAndRun()
//    {
//        var graphHostService = Build();
//        var startOption = await graphHostService.Services.StartGraphEngine(GraphMap);
//        startOption.ThrowOnError("Failed to start");

//        var context = graphHostService.Services.GetRequiredService<ILogger<GraphHostBuilder>>().ToScopeContext();
//        context.LogWarning("Run engine");

//        return graphHostService;
//    }

//    private IConfiguration ReadConfiguration()
//    {
//        var configuration = new ConfigurationBuilder()
//            .AddJsonFile(ConfigurationFile.NotEmpty())
//            .Build();

//        string? secretName = configuration["SecretName"];
//        if (secretName == null) return configuration;

//        configuration = new ConfigurationBuilder()
//            .AddJsonFile(ConfigurationFile)
//            .AddUserSecrets(secretName)
//            .Build();

//        return configuration;
//    }
//}

