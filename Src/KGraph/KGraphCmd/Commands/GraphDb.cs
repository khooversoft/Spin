using KGraphCmd.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace KGraphCmd.Commands;

internal class GraphDb : ICommandRoute
{
    private readonly AbortSignal _abortSignal;
    private readonly ILogger<TraceLog> _logger;
    private readonly ScopeContext _context;
    private readonly GraphHostManager _graphHostManager;

    public GraphDb(GraphHostManager graphHostManager, AbortSignal abortSignal, ILogger<TraceLog> logger)
    {
        _abortSignal = abortSignal.NotNull();
        _logger = logger.NotNull();
        _context = new ScopeContext(_logger);
        _graphHostManager = graphHostManager;
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("db", "Query or reset KGraph's database files")
    {
        new CommandSymbol("nodes", "Dump all the nodes").Action(x =>
        {
            var jsonFile = x.AddOption<string?>("--config", "Json file with data lake connection details");
            x.SetHandler(DumpNodes, jsonFile);
        }),
        new CommandSymbol("edges", "Dump all the edges").Action(x =>
        {
            var jsonFile = x.AddOption<string?>("--config", "Json file with data lake connection details");
            x.SetHandler(DumpEdges, jsonFile);
        }),
    };

    private Task DumpNodes(string? jsonFile)
    {
        if (jsonFile.IsNotEmpty()) _graphHostManager.Start(jsonFile);

        IGraphHost graphHost = _graphHostManager.ServiceProvider.GetRequiredService<IGraphHost>();
        _context.LogInformation("Dumping nodes, count={count}", graphHost.Map.Nodes.Count);

        foreach (var node in graphHost.Map.Nodes.OrderBy(x => x.Key))
        {
            _context.LogInformation(node.ToString());
        }

        return Task.CompletedTask;
    }


    private Task DumpEdges(string? jsonFile)
    {
        if (jsonFile.IsNotEmpty()) _graphHostManager.Start(jsonFile);

        IGraphHost graphHost = _graphHostManager.ServiceProvider.GetRequiredService<IGraphHost>();
        _context.LogInformation("Dumping edges, count={count}", graphHost.Map.Edges.Count);

        foreach (var node in graphHost.Map.Edges.OrderBy(x => x.ToString()))
        {
            _context.LogInformation(node.ToString());
        }

        return Task.CompletedTask;
    }
}
