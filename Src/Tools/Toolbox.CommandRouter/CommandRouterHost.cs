using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Option = Toolbox.Types.Option;

namespace Toolbox.CommandRouter;

public interface ICommandRouterHost
{
    Task<Option> Run(ScopeContext context, params string[] args);
}


public class CommandRouterHost : ICommandRouterHost
{
    private readonly string _commandId;
    private readonly ILogger<CommandRouterHost> _logger;
    private readonly IServiceProvider _service;

    public CommandRouterHost(IServiceProvider serviceProvider, string commandId, ILogger<CommandRouterHost> logger)
    {
        _service = serviceProvider.NotNull();
        _commandId = commandId.NotEmpty();
        _logger = logger.NotNull();
    }

    public async Task<Option> Run(ScopeContext context, params string[] args)
    {
        context = context.With(_logger);
        context.LogDebug("Run: commandId={commandId}, Args='{args}'", _commandId, args.Join(','));

        IReadOnlyList<CommandSymbol> commandRoutes = _service
            .GetKeyedServices<ICommandRoute>(_commandId)
            .Select(x => x.CommandSymbol())
            .ToArray();

        try
        {
            using (var scope = _service.NotNull().CreateScope())
            {
                int rcResult = 0;
                var capture = new ConsoleCapture();

                var rc = new RootCommand();
                commandRoutes.ForEach(x => rc.AddCommand(x.Command));

                rcResult = await rc.InvokeAsync(args, console: capture);
                if (rcResult != 0) context.LogError("CommandId={commandId}, Args='{args}' failed", _commandId, args.Join(" "));

                capture.Dump(context);
                return rcResult != 0 ? StatusCode.BadRequest : StatusCode.OK;
            }
        }
        catch (Exception ex)
        {
            context.LogError(ex, "Failed to execute commandId={commandId}", _commandId);
            return StatusCode.BadRequest;
        }
    }
}
