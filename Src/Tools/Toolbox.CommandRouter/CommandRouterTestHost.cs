using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace Toolbox.CommandRouter;

public class CommandRouterTestHost
{
    private IConfigurationBuilder _configBuilder;
    private IServiceCollection _serviceCollection;
    private const string _commandId = "test";

    public CommandRouterTestHost()
    {
        _configBuilder = new ConfigurationBuilder();

        _serviceCollection = new ServiceCollection()
            .AddLogging(config =>
            {
                config.SimpleConsole();
                config.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
            })
            .AddCommandHost(_commandId);
    }

    public CommandRouterTestHost AddCommand<T>() where T : class, ICommandRoute
    {
        _serviceCollection.AddCommand<T>(_commandId);
        return this;
    }

    public CommandRouterTestHost ConfigureAppConfiguration(Action<IConfigurationBuilder> builder)
    {
        builder(_configBuilder);
        return this;
    }

    public CommandRouterTestHost ConfigureAppConfiguration(Action<IConfigurationBuilder, IServiceCollection> builder)
    {
        builder(_configBuilder, _serviceCollection);
        return this;
    }

    public CommandRouterTestHost ConfigureLogging(Action<ILoggingBuilder> configureLogging)
    {
        _serviceCollection.AddLogging(configureLogging);
        return this;
    }

    public CommandRouterTestHost ConfigureService(Action<IServiceCollection> setup)
    {
        setup(_serviceCollection);
        return this;
    }

    public CommandRouterTestHostBuild Build()
    {
        IConfiguration config = _configBuilder.Build();
        _serviceCollection.AddSingleton(config);

        ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider();
        var host = serviceProvider.GetCommandRouterHost(_commandId);

        return new CommandRouterTestHostBuild(serviceProvider, _commandId, host);
    }
}

public readonly struct CommandRouterTestHostBuild
{
    public CommandRouterTestHostBuild(IServiceProvider serviceProvider, string commandId, ICommandRouterHost commandRouterHost)
    {
        ServiceProvider = serviceProvider;
        CommandId = commandId;
        Command = commandRouterHost;
    }

    public IServiceProvider ServiceProvider { get; }
    public string CommandId { get; }
    public ICommandRouterHost Command { get; }
}
