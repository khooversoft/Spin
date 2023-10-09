using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.CommandRouter;

public sealed record CommandRouterHostArgs
{
}

public class CommandRouterHost
{
    private readonly IReadOnlyList<Func<IServiceProvider, Task>> _startup;
    private readonly IReadOnlyList<Func<IServiceProvider, Task>> _shutdown;
    private string[] _args;
    public CommandRouterHost(string[] args, IServiceProvider service, IEnumerable<Func<IServiceProvider, Task>> startup, IEnumerable<Func<IServiceProvider, Task>> shutdown)
    {
        _args = args.NotNull().ToArray();
        Service = service.NotNull();
        _startup = startup.ToArray();
        _shutdown = shutdown.ToArray();
    }

    public IReadOnlyList<string> Args => _args;
    public IServiceProvider Service { get; }

    public async Task<int> Run()
    {
        IReadOnlyList<CommandSymbol> commandRoutes = Service
            .GetServices<ICommandRoute>()
            .Select(x => x.CommandSymbol())
            .ToArray();

        ScopeContext context = Service
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger<CommandRouterHost>()
            .Func(x => new ScopeContext(x));

        try
        {
            AbortSignal? abortSignal = Service.GetService<AbortSignal>();
            abortSignal?.StartTracking();

            await _startup.ForEachAsync(async x => await x(Service));

            var rc = new RootCommand();

            commandRoutes.ForEach(x =>
            {
                rc.AddCommand(x.Command);
            });

            await rc.InvokeAsync(_args);

            await _shutdown.ForEachAsync(async x => await x(Service));

            abortSignal?.StopTracking();
            int state = abortSignal?.GetToken().IsCancellationRequested == true ? 1 : 0;

            context.Trace().LogInformation("Command completed, state={state}", state);
            return state;
        }
        catch (Exception ex)
        {
            context.Trace().LogError(ex, "Failed to execute command");
            return 1;
        }
    }
}

