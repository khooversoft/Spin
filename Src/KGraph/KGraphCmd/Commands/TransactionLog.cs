using KGraphCmd.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Journal;
using Toolbox.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace KGraphCmd.Commands;

internal class TransactionLog : ICommandRoute
{
    private readonly AbortSignal _abortSignal;
    private readonly ILogger<TraceLog> _logger;
    private readonly GraphHostManager _graphHostManager;

    public TransactionLog(GraphHostManager graphHostManager, AbortSignal abortSignal, ILogger<TraceLog> logger)
    {
        _graphHostManager = graphHostManager.NotNull();
        _abortSignal = abortSignal.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("trx", "Dump or reset KGraph's database transaction logs")
    {
        new CommandSymbol("list", "List transactions").Action(x =>
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
        context.LogInformation("Starting to list transactions...");

        var traceLog = _graphHostManager.ServiceProvider.GetRequiredKeyedService<IJournalFile>(GraphConstants.TrxJournal.DiKeyed).NotNull();
        await JournalTool.Display(traceLog, monitor, lastNumber, fullDump, lsn, context);
    }
}
