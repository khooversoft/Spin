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
            var fullDump = x.AddOption<bool>("--full", "Json file with data lake connection details");
            var nodeKey = x.AddOption<string?>("--nodeKey", "Enter node key to search for");

            x.SetHandler(DumpNodes, jsonFile, fullDump, nodeKey);
        }),
        new CommandSymbol("edges", "Dump all the edges").Action(x =>
        {
            var jsonFile = x.AddOption<string?>("--config", "Json file with data lake connection details");
            var fullDump = x.AddOption<bool>("--full", "Json file with data lake connection details");
            var fromKey = x.AddOption<string?>("--fromKey", "Enter node key to search for");
            var toKey = x.AddOption<string?>("--toKey", "Enter node key to search for");
            var edgeType = x.AddOption<string?>("--edgeType", "Enter node key to search for");

            x.SetHandler(DumpEdges, jsonFile, fullDump, fromKey, toKey, edgeType);
        }),
        new CommandSymbol("query", "Execute kgraph command").Action(x =>
        {
            var jsonFile = x.AddOption<string?>("--config", "Json file with data lake connection details");
            var fullDump = x.AddOption<bool>("--full", "Json file with data lake connection details");
            var command = x.AddArgument<string>("graph command", "kgraph command to execute");

            x.SetHandler(Query, jsonFile, command, fullDump);
        }),
    };

    private async Task Query(string? jsonFile, string command, bool fullDump)
    {
        if (jsonFile.IsNotEmpty()) _graphHostManager.Start(jsonFile);
        var context = new ScopeContext(_logger, _abortSignal.GetToken());
        await _graphHostManager.LoadMap(context);

        var client = _graphHostManager.ServiceProvider.GetRequiredService<IGraphClient>();
        var result = await client.ExecuteBatch(command, context);
        if (result.IsError())
        {
            result.LogStatus(context, "Failed to execute command '{cmd}'", [command]);
            return;
        }

        QueryBatchResult queryResult = result.Return();

        DataFormatType dataFormatType = fullDump ? DataFormatType.Full : DataFormatType.Single;
        var line = DataFormatTool.Formats.Format(queryResult, dataFormatType).ToLoggingFormat();
        context.LogInformation(line);
    }

    private async Task DumpNodes(string? jsonFile, bool fullDump, string? nodeKey)
    {
        if (jsonFile.IsNotEmpty()) _graphHostManager.Start(jsonFile);
        var context = new ScopeContext(_logger, _abortSignal.GetToken());
        await _graphHostManager.LoadMap(context);

        IGraphHost graphHost = _graphHostManager.ServiceProvider.GetRequiredService<IGraphHost>();
        context.LogInformation("Dumping nodes, count={count}", graphHost.Map.Nodes.Count);

        DataFormatType dataFormatType = fullDump ? DataFormatType.Full : DataFormatType.Single;

        var list = graphHost.Map.Nodes
            .Where(x => nodeKey == null || x.Key.Equals(nodeKey, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Key);

        foreach (var node in list)
        {
            var line = DataFormatTool.Formats.Format(node, dataFormatType).ToLoggingFormat();
            context.LogInformation(line);
        }
    }

    private async Task DumpEdges(string? jsonFile, bool fullDump, string? fromKey, string? toKey, string? edgeType)
    {
        if (jsonFile.IsNotEmpty()) _graphHostManager.Start(jsonFile);
        var context = new ScopeContext(_logger, _abortSignal.GetToken());
        await _graphHostManager.LoadMap(context);

        IGraphHost graphHost = _graphHostManager.ServiceProvider.GetRequiredService<IGraphHost>();
        context.LogInformation("Dumping edges, count={count}", graphHost.Map.Edges.Count);

        DataFormatType dataFormatType = fullDump ? DataFormatType.Full : DataFormatType.Single;

        var list = graphHost.Map.Edges
            .Where(x => fromKey == null || x.FromKey.Equals(fromKey, StringComparison.OrdinalIgnoreCase))
            .Where(x => toKey == null || x.FromKey.Equals(toKey, StringComparison.OrdinalIgnoreCase))
            .Where(x => edgeType == null || x.FromKey.Equals(edgeType, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.ToString());

        foreach (var node in list)
        {
            var line = DataFormatTool.Formats.Format(node, dataFormatType).ToLoggingFormat();
            context.LogInformation(line);
        }
    }
}
