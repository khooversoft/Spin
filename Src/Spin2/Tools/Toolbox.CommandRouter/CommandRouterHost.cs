using System.Collections.Concurrent;
using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.CommandRouter;

public class CommandRouterHost : ICommandRouterHost
{
    private ConcurrentQueue<string[]> _argsQueue = new();

    public CommandRouterHost(string[] args, IServiceCollection serviceCollection)
    {
        Enqueue(args);
        serviceCollection.NotNull().AddSingleton<ICommandRouterHost>(this);
        Service = serviceCollection.BuildServiceProvider();
    }

    public IServiceProvider Service { get; }

    public void Enqueue(params string[] args) => _argsQueue.Enqueue(args.NotNull());

    public async Task<int> Run(params string[] args)
    {
        _argsQueue.Clear();
        Enqueue(args);

        return await Run();
    }

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

            int rcResult = 0;
            var capture = new ConsoleCapature(context.Location());

            while (_argsQueue.TryDequeue(out var args))
            {
                var rc = new RootCommand();
                commandRoutes.ForEach(x => rc.AddCommand(x.Command));

                rcResult = await rc.InvokeAsync(args, capture);
                if (rcResult != 0)
                {
                    context.Trace().LogError("Args={args} failed", args.Join(" "));
                    break;
                }
            }

            capture.Dump();
            abortSignal?.StopTracking();
            int state = abortSignal?.GetToken().IsCancellationRequested == true ? 1 : 0;

            if (state != 0 || rcResult != 0)
            {
                context.Trace().LogError("Command failed, state={state}, rcResult={rcResult}", state, rcResult);
                return 1;
            }

            context.Trace().LogTrace("Command completed, state={state}, rcResult={rcResult}", state, rcResult);
            return 0;
        }
        catch (Exception ex)
        {
            context.Trace().LogError(ex, "Failed to execute command");
            return 1;
        }
    }
}
