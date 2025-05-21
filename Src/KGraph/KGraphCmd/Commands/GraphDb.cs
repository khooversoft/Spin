using KGraphCmd.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace KGraphCmd.Commands;

internal class GraphDb : ICommandRoute
{
    private readonly ILogger<GraphDb> _logger;
    private readonly GraphHostManager _graphHostManager;

    public GraphDb(GraphHostManager graphHostManager, ILogger<GraphDb> logger)
    {
        _logger = logger.NotNull();
        _graphHostManager = graphHostManager;
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("db", "KGraph database utilities")
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
    };

    private async Task DumpNodes(string? jsonFile, bool fullDump, string? nodeKey)
    {
        if (jsonFile.IsNotEmpty()) await _graphHostManager.Start(jsonFile);
        var context = _logger.ToScopeContext();

        IGraphEngine graphEngine = _graphHostManager.ServiceProvider.GetRequiredService<IGraphEngine>();
        context.LogInformation("Dumping nodes, count={count}", graphEngine.GetMapData().NotNull().Map.Nodes.Count);

        DataFormatType dataFormatType = fullDump ? DataFormatType.Full : DataFormatType.Single;

        var list = graphEngine.GetMapData().NotNull().Map.Nodes
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
        if (jsonFile.IsNotEmpty()) await _graphHostManager.Start(jsonFile);
        var context = _logger.ToScopeContext();
        //await _graphHostManager.LoadMap(context);

        IGraphEngine graphHost = _graphHostManager.ServiceProvider.GetRequiredService<IGraphEngine>();
        context.LogInformation("Dumping edges, count={count}", graphHost.GetMapData().NotNull().Map.Edges.Count);

        DataFormatType dataFormatType = fullDump ? DataFormatType.Full : DataFormatType.Single;

        var list = graphHost.GetMapData().NotNull().Map.Edges
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
