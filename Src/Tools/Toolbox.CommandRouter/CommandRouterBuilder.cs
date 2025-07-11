using Microsoft.Extensions.DependencyInjection;
using Toolbox.Tools;

namespace Toolbox.CommandRouter;

public static class CommandRouterBuilder
{
    public static IServiceCollection AddCommand<T>(this IServiceCollection services, string commandId) where T : class, ICommandRoute
    {
        services.NotNull();
        commandId.NotEmpty("Required");

        services.NotNull().AddKeyedScoped<ICommandRoute, T>(commandId.NotEmpty("Required"));
        return services;
    }

    public static IServiceCollection AddCommandHost(this IServiceCollection services, string commandId)
    {
        services.NotNull();
        commandId.NotEmpty("Required");

        services.AddKeyedScoped<ICommandRouterHost, CommandRouterHost>(commandId, (s, _) => ActivatorUtilities.CreateInstance<CommandRouterHost>(s, commandId));
        return services;
    }

    public static CommandCollectionContext AddCommandCollection(this IServiceCollection services, string commandId)
    {
        services.NotNull();
        commandId.NotEmpty("Required");

        services.AddCommandHost(commandId);
        services.AddKeyedScoped<ICommandRouterHost, CommandRouterHost>(commandId, (s, _) => ActivatorUtilities.CreateInstance<CommandRouterHost>(s, commandId));

        return new CommandCollectionContext(services, commandId);
    }

    public static ICommandRouterHost GetCommandRouterHost(this IServiceProvider serviceProvider, string commandId)
    {
        serviceProvider.NotNull();
        commandId.NotEmpty("Required");

        serviceProvider.NotNull().NotNull(commandId, nameof(commandId));
        return serviceProvider.GetRequiredKeyedService<ICommandRouterHost>(commandId);
    }
}

public readonly struct CommandCollectionContext
{
    private readonly IServiceCollection _serviceCollection;
    private readonly string _commandId;

    public CommandCollectionContext(IServiceCollection serviceCollection, string commandId)
    {
        _serviceCollection = serviceCollection.NotNull();
        _commandId = commandId.NotEmpty(); ;
    }

    public CommandCollectionContext AddCommand<T>() where T : class, ICommandRoute
    {
        _serviceCollection.AddKeyedScoped<ICommandRoute, T>(_commandId);
        return this;
    }

    public CommandCollectionContext AddCommand<T>(Func<IServiceProvider, string, T> value) where T : class, ICommandRoute
    {
        _serviceCollection.AddKeyedScoped<ICommandRoute, T>(_commandId, (s, k) => value(s, (string)k));
        return this;
    }

    public IServiceCollection Return() => _serviceCollection;
}
