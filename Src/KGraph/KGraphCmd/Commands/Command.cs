using System.Collections.Frozen;
using KGraphCmd.Application;
using Microsoft.Extensions.Logging;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;

namespace KGraphCmd.Commands;

internal class Command : ICommandRoute
{
    private static FrozenSet<string> _exitCommands = new HashSet<string>()
    {
        "exit", "quit", "q", "ex"
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    private readonly AbortSignal _abortSignal;
    private readonly ILogger<Command> _logger;
    private readonly ScopeContext _context;
    private readonly ICommandRouterHost _commandHost;
    private readonly GraphHostManager _graphHostManager;

    private readonly StringTokenizer _argsTokenizer = new StringTokenizer()
        .UseCollapseWhitespace()
        .UseSingleQuote()
        .UseDoubleQuote();

    public Command(ICommandRouterHost commandHost, GraphHostManager graphHostManager, AbortSignal abortSignal, ILogger<Command> logger)
    {
        _commandHost = commandHost.NotNull();
        _abortSignal = abortSignal.NotNull();
        _logger = logger.NotNull();
        _context = new ScopeContext(_logger);
        _graphHostManager = graphHostManager.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("command", "Dump or reset KGraph's database traces")
    {
        new CommandSymbol("run", "Run commands").Action(x =>
        {
            var jsonFile = x.AddOption<string>("--config", "Json file with data lake connection details");
            x.SetHandler(Run, jsonFile);
        }),
    };

    private async Task Run(string jsonFile)
    {
        var services = _graphHostManager.Start(jsonFile);
        _context.LogInformation("Starting command shell...");

        //var traceLog = services.GetRequiredKeyedService<IJournalFile>(GraphConstants.Trace.DiKeyed).NotNull();
        var hashLsn = new HashSet<string>();

        await Task.Delay(TimeSpan.FromMilliseconds(100));

        while (!_abortSignal.GetToken().IsCancellationRequested)
        {
            Console.Write("> ");

            string command = Console.ReadLine() ?? string.Empty;
            if (command.IsEmpty())
            {
                Console.WriteLine("'exit' to exit, -? for help");
                Console.WriteLine();
                continue;
            }

            var args = parse(command);
            if (args.Length == 0) continue;

            if (args.Length == 1 && _exitCommands.Contains(args[0])) break;

            var result = await _commandHost.Run(args);
            Console.WriteLine("...");
        }

        string[] parse(string command)
        {
            try
            {
                var args = _argsTokenizer.Parse(command)
                    .Select(x => x.Value)
                    .Where(x => x.IsNotEmpty())
                    .ToArray();

                return args;
            }
            catch
            {
                Console.WriteLine("Syntax error");
            }

            return Array.Empty<string>();
        }
    }
}
