using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KGraphCmd.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Journal;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;

namespace KGraphCmd.Commands;

internal class Command : ICommandRoute
{
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
        _context.LogInformation("Starting to list traces...");

        var traceLog = services.GetRequiredKeyedService<IJournalFile>(GraphConstants.Trace.DiKeyed).NotNull();
        var hashLsn = new HashSet<string>();

        while (!_abortSignal.GetToken().IsCancellationRequested)
        {
            Console.Write("> ");

            string command = Console.ReadLine() ?? string.Empty;
            if (command.IsEmpty()) continue;

            var args = _argsTokenizer.Parse(command)
                .Select(x => x.Value)
                .Where(x => x.IsNotEmpty())
                .ToArray();

            if( args.Length == 1 && args[0] == "exit") break;

            var result = await _commandHost.Run(args);
            Console.WriteLine();
        }
    }
}
