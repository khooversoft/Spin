//using System.Collections.Frozen;
//using KGraphCmd.Application;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Toolbox.CommandRouter;
//using Toolbox.Extensions;
//using Toolbox.LangTools;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace KGraphCmd.Commands;

//internal class Command : ICommandRoute
//{
//    private static FrozenSet<string> _exitCommands = new HashSet<string>()
//    {
//        "exit", "quit", "q", "ex"
//    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

//    private readonly ILogger<Command> _logger;
//    private readonly ScopeContext _context;
//    private readonly ICommandRouterHost _commandHost;
//    private readonly GraphHostManager _graphHostManager;

//    private static readonly StringTokenizer _argsTokenizer = new StringTokenizer()
//        .UseCollapseWhitespace()
//        .UseSingleQuote()
//        .UseDoubleQuote();

//    public Command([FromKeyedServices("run")] ICommandRouterHost commandHost, GraphHostManager graphHostManager, ILogger<Command> logger)
//    {
//        _commandHost = commandHost.NotNull();
//        _logger = logger.NotNull();
//        _context = new ScopeContext(_logger);
//        _graphHostManager = graphHostManager.NotNull();
//    }

//    public CommandSymbol CommandSymbol() => new CommandSymbol("command", "KGraph utilities")
//    {
//        new CommandSymbol("run", "Run commands").Action(x =>
//        {
//            var jsonFile = x.AddOption<string>("--config", "Json file with data lake connection details", isRequired: true);
//            x.SetHandler(Run, jsonFile);
//        }),
//    };

//    private async Task Run(string jsonFile)
//    {
//        var context = _logger.ToScopeContext();

//        _context.LogInformation("Starting command shell...");
//        await Task.Delay(TimeSpan.FromMilliseconds(100));

//        while (true)
//        {
//            await _graphHostManager.Start(jsonFile);

//            Console.Write("> ");

//            string command = Console.ReadLine() ?? string.Empty;
//            if (command.IsEmpty())
//            {
//                Console.WriteLine($"{_exitCommands.Join(", ")} to exit, -? for help");
//                Console.WriteLine();
//                continue;
//            }

//            var args = parse(command);
//            if (args.Length == 0) continue;

//            if (args.Length == 1 && _exitCommands.Contains(args[0])) break;

//            var result = await _commandHost.Run(context, args);
//            Console.WriteLine("...");
//        }

//        string[] parse(string command)
//        {
//            try
//            {
//                var args = _argsTokenizer.Parse(command)
//                    .Select(x => x.Value)
//                    .Where(x => x.IsNotEmpty())
//                    .ToArray();

//                return args;
//            }
//            catch { }

//            Console.WriteLine("Syntax error");
//            return Array.Empty<string>();
//        }
//    }
//}
