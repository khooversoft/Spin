using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace Toolbox.CommandRouter;

public class CommandRouterBuilder
{
    private IConfigurationBuilder _configBuilder;
    private IServiceCollection _serviceCollection;
    private string[]? _args;
    private IList<Func<IServiceProvider, Task>> _startup = new List<Func<IServiceProvider, Task>>();
    private IList<Func<IServiceProvider, Task>> _shutdown = new List<Func<IServiceProvider, Task>>();

    public CommandRouterBuilder()
    {
        _configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false);

        _serviceCollection = new ServiceCollection()
            .AddLogging(config => config.AddConsole())
            .AddSingleton<AbortSignal>();
    }

    public CommandRouterBuilder AddCommand<T>() where T : class, ICommandRoute
    {
        _serviceCollection.AddCommandRoute<T>();
        return this;
    }

    public CommandRouterBuilder ConfigureAppConfiguration(Action<IConfigurationBuilder> builder)
    {
        builder(_configBuilder);
        return this;
    }

    public CommandRouterBuilder ConfigureAppConfiguration(Action<IConfigurationBuilder, IServiceCollection> builder)
    {
        builder(_configBuilder, _serviceCollection);
        return this;
    }

    public CommandRouterBuilder ConfigureLogging(Action<ILoggingBuilder> configureLogging)
    {
        _serviceCollection.AddLogging(configureLogging);
        return this;
    }

    public CommandRouterBuilder ConfigureService(Action<IServiceCollection> setup)
    {
        setup(_serviceCollection);
        return this;
    }

    public CommandRouterBuilder SetArgs(string[] args)
    {
        (string[] ConfigArgs, string[] CommandLineArgs) = ArgumentTool.Split(args);

        _configBuilder.AddCommandLine(ConfigArgs);
        _args = CommandLineArgs;
        return this;
    }

    public CommandRouterBuilder AddShutdown(Func<IServiceProvider, Task> shutdown)
    {
        _shutdown.Add(shutdown.NotNull());
        return this;
    }

    public CommandRouterBuilder AddStartup(Func<IServiceProvider, Task> startup)
    {
        _startup.Add(startup.NotNull());
        return this;
    }

    public CommandRouterHost Build()
    {
        IConfiguration config = _configBuilder.Build();
        _serviceCollection.AddSingleton(config);

        IServiceProvider service = _serviceCollection.BuildServiceProvider();
        return new CommandRouterHost(_args ?? Array.Empty<string>(), service, _startup, _shutdown);
    }
}
