using KGraphCmd.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace KGraphCmd.Commands;

internal class GraphDb : ICommandRoute
{
    private readonly AbortSignal _abortSignal;
    private readonly ILogger<TraceLog> _logger;
    private readonly GraphHostManager _graphHostManager;

    public GraphDb(GraphHostManager graphHostManager, AbortSignal abortSignal, ILogger<TraceLog> logger)
    {
        _abortSignal = abortSignal.NotNull();
        _logger = logger.NotNull();
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
        new CommandSymbol("query", "Execute kgraph command").Action(x =>
        {
            var jsonFile = x.AddOption<string?>("--config", "Json file with data lake connection details");
            var command = x.AddArgument<string>("graph command", "kgraph command to execute");
            x.SetHandler(Query, jsonFile, command);
        }),
    };

    private async Task Query(string? jsonFile, string command)
    {
        if (jsonFile.IsNotEmpty()) _graphHostManager.Start(jsonFile);
        var context = new ScopeContext(_logger, _abortSignal.GetToken());
        await _graphHostManager.LoadMap(context);

        var client = _graphHostManager.ServiceProvider.GetRequiredService<IGraphClient>();
        context.LogInformation("Executing '{cmd}", command);

        var result = await client.Execute(command, context);
        if (result.IsError())
        {
            result.LogStatus(context, "Failed to execute command '{cmd}'", [command]);
            return;
        }

        QueryResult queryResult = result.Return();
        string details = queryResult.DumpToString();
        context.LogInformation("Results: {details}", details);
    }

    private async Task DumpNodes(string? jsonFile)
    {
        if (jsonFile.IsNotEmpty()) _graphHostManager.Start(jsonFile);
        var context = new ScopeContext(_logger, _abortSignal.GetToken());
        await _graphHostManager.LoadMap(context);

        IGraphHost graphHost = _graphHostManager.ServiceProvider.GetRequiredService<IGraphHost>();
        context.LogInformation("Dumping nodes, count={count}", graphHost.Map.Nodes.Count);

        foreach (var node in graphHost.Map.Nodes.OrderBy(x => x.Key))
        {
            var line = node.GetProperties().ToLoggingFormat();
            context.LogInformation(line);
        }
    }

    private async Task DumpEdges(string? jsonFile)
    {
        if (jsonFile.IsNotEmpty()) _graphHostManager.Start(jsonFile);
        var context = new ScopeContext(_logger, _abortSignal.GetToken());
        await _graphHostManager.LoadMap(context);

        IGraphHost graphHost = _graphHostManager.ServiceProvider.GetRequiredService<IGraphHost>();
        context.LogInformation("Dumping edges, count={count}", graphHost.Map.Edges.Count);

        foreach (var edge in graphHost.Map.Edges.OrderBy(x => x.ToString()))
        {
            var line = edge.GetProperties().ToLoggingFormat();
            context.LogInformation(line);
        }
    }
}
