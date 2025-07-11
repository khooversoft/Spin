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
    private readonly ScopeContext _context;

    public QueryDb(GraphHostManager graphHostManager, ILogger<QueryDb> logger)
    {
        _graphHostManager = graphHostManager;
        _logger = logger.NotNull();
        _context = _logger.ToScopeContext();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("query", "Query KGraph database").Action(x =>
    {
        var jsonFile = x.AddOption<string?>("--config", "Json file with data lake connection details");
        var fullDump = x.AddOption<bool>("--full", "Json file with data lake connection details");
        var dumpData = x.AddOption<bool>("--dump", "Dump data to file in working folder");
        var command = x.AddArgument<string>("graph command", "KGraph command to execute");

        x.SetHandler(ExecuteQuery, jsonFile, command, fullDump, dumpData);
    });

    private async Task ExecuteQuery(string? jsonFile, string command, bool fullDump, bool dumpData)
    {
        if (jsonFile.IsNotEmpty()) await _graphHostManager.Start(jsonFile);

        var client = _graphHostManager.ServiceProvider.GetRequiredService<IGraphClient>();
        var result = await client.ExecuteBatch(command, _context);
        if (result.IsError())
        {
            result.LogStatus(_context, "Failed to execute command '{cmd}'", [command]);
            return;
        }

        QueryBatchResult queryResult = result.Return();

        QueryBatchResult trimmed = queryResult with
        {
            Items = queryResult.Items.Select(x => x with
            {
                DataLinks = x.DataLinks.Select(y => y with { Data = new DataETag([]) }).ToArray(),
            }).ToArray(),
        };

        await dumpLinkData();

        if (!fullDump)
        {
            var singleLine = DataFormatTool.Formats.Format(trimmed, DataFormatType.Single).ToLoggingFormat();
            _context.LogInformation(singleLine);
            return;
        }

        var line = "".ToEnumerable()
            .Append(trimmed.ToJsonFormat())
            .Join(Environment.NewLine);

        _context.LogInformation("Batch result... {line}", line);


        async Task dumpLinkData()
        {
            if (!dumpData || _graphHostManager.DumpFolder.IsEmpty()) return;

            var linedDataItems = queryResult.Items.SelectMany(x => x.DataLinks).ToArray();

            foreach (var item in linedDataItems)
            {
                string file = Path.Combine(_graphHostManager.DumpFolder, Path.GetFileName(item.FileId));
                string data = item.Data.ToJsonFromData();
                await File.WriteAllTextAsync(file, data);
                _context.LogInformation("Dumped data entity={entity} to {file}", item.Name, file);
            }
        }
    }
}