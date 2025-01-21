using KGraphCmd.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Journal;
using Toolbox.Tools;
using Toolbox.Types;

namespace KGraphCmd.Commands;

internal class TraceLog : ICommandRoute
{
    private readonly AbortSignal _abortSignal;
    private readonly ILogger<TraceLog> _logger;
    private readonly GraphHostManager _graphHostManager;

    public TraceLog(GraphHostManager graphHostManager, AbortSignal abortSignal, ILogger<TraceLog> logger)
    {
        _graphHostManager = graphHostManager.NotNull();
        _abortSignal = abortSignal.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("trace", "Dump or reset KGraph's database traces")
    {
        new CommandSymbol("list", "List traces").Action(x =>
        {
            var jsonFile = x.AddOption<string?>("--config", "Json file with data lake connection details");
            var monitor = x.AddOption<bool>("--monitor", "Monitor traces");
            var lastNumber = x.AddOption<int>("--last", "Last number of traces to show", 10);
            var fullDump = x.AddOption<bool>("--full", "Full dump of data");
            var lsn = x.AddOption<string?>("--lsn", "Display LSN details");

            x.SetHandler(List, jsonFile, monitor, lastNumber, fullDump, lsn);
        }),
    };

    private async Task List(string? jsonFile, bool monitor, int lastNumber, bool fullDump, string? lsn)
    {
        if (jsonFile.IsNotEmpty()) _graphHostManager.Start(jsonFile);

        var context = new ScopeContext(_logger, _abortSignal.GetToken());
        context.LogInformation("Starting to list traces...");

        var traceLog = _graphHostManager.ServiceProvider.GetRequiredKeyedService<IJournalFile>(GraphConstants.Trace.DiKeyed).NotNull();
        await JournalTool.Display(traceLog, monitor, lastNumber, fullDump, lsn, context);
    }
}
