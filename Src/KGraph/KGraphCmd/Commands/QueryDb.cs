using KGraphCmd.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace KGraphCmd.Commands;

internal class QueryDb : ICommandRoute
{
    private readonly ILogger<QueryDb> _logger;
    private readonly GraphHostManager _graphHostManager;

    public QueryDb(GraphHostManager graphHostManager, ILogger<QueryDb> logger)
    {
        _logger = logger.NotNull();
        _graphHostManager = graphHostManager;
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("query", "Query KGraph database").Action(x =>
    {
        var jsonFile = x.AddOption<string?>("--config", "Json file with data lake connection details");
        var fullDump = x.AddOption<bool>("--full", "Json file with data lake connection details");
        var command = x.AddArgument<string>("graph command", "KGraph command to execute");

        x.SetHandler(ExecuteQuery, jsonFile, command, fullDump);
    });

    private async Task ExecuteQuery(string? jsonFile, string command, bool fullDump)
    {
        if (jsonFile.IsNotEmpty()) await _graphHostManager.Start(jsonFile);
        var context = _logger.ToScopeContext();
        //await _graphHostManager.LoadMap(context);

        var client = _graphHostManager.ServiceProvider.GetRequiredService<IGraphClient>();
        var result = await client.ExecuteBatch(command, context);
        if (result.IsError())
        {
            result.LogStatus(context, "Failed to execute command '{cmd}'", [command]);
            return;
        }

        QueryBatchResult queryResult = result.Return();
        GraphLinkData[] saveLinkData = queryResult.Items.SelectMany(x => x.DataLinks).ToArray();
        QueryBatchResult trimmed = queryResult with
        {
            Items = queryResult.Items.Select(x => x with
            {
                DataLinks = x.DataLinks.Select(y => y with { Data = new DataETag([]) }).ToArray(),
            }).ToArray(),
        };

        DataFormatType dataFormatType = fullDump ? DataFormatType.Full : DataFormatType.Single;
        var line = DataFormatTool.Formats.Format(trimmed, dataFormatType).ToLoggingFormat();
        context.LogInformation(line);

        foreach (var item in saveLinkData)
        {
            string l2 = DataFormatTool.Formats.Format(item, DataFormatType.Single).ToLoggingFormat();
            context.LogInformation(l2);
        }
    }
}