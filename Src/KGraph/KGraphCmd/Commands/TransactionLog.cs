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
    private readonly ScopeContext _context;
    private readonly GraphHostManager _graphHostManager;

    public TransactionLog(GraphHostManager graphHostManager, AbortSignal abortSignal, ILogger<TraceLog> logger)
    {
        _graphHostManager = graphHostManager.NotNull();
        _abortSignal = abortSignal.NotNull();
        _logger = logger.NotNull();
        _context = new ScopeContext(_logger);
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("trx", "Dump or reset KGraph's database transaction logs")
    {
        new CommandSymbol("list", "List transactions").Action(x =>
        {
            var jsonFile = x.AddOption<string?>("--config", "Json file with data lake connection details");
            var monitor = x.AddOption<bool>("--monitor", "Monitor traces");
            x.SetHandler(List, jsonFile, monitor);
        }),
    };

    private async Task List(string? jsonFile, bool monitor)
    {
        if (jsonFile.IsNotEmpty()) _graphHostManager.Start(jsonFile);

        _context.LogInformation("Starting to list transactions...");

        var traceLog = _graphHostManager.ServiceProvider.GetRequiredKeyedService<IJournalFile>(GraphConstants.TrxJournal.DiKeyed).NotNull();
        var hashLsn = new HashSet<string>();

        while (!_abortSignal.GetToken().IsCancellationRequested)
        {
            await ReadJournal(traceLog, hashLsn, _context);
            if (!monitor) return;

            await InputTool.WaitForInput(async () => await ReadJournal(traceLog, hashLsn, _context), _abortSignal.GetToken());

            var match = InputTool.GetUserCommand(_abortSignal.GetToken(), "continue", "quit", "reset");
            switch (match)
            {
                case "continue": continue;
                case "quit": return;

                case "reset":
                    hashLsn.Clear();
                    continue;

                default: return;
            }
        }
    }

    private async Task ReadJournal(IJournalFile journalFile, HashSet<string> hashLsn, ScopeContext context)
    {
        context.LogTrace("Reading transactions...");
        IReadOnlyList<JournalEntry> entries = await journalFile.ReadJournals(context);

        context.LogTrace("Read transaction log, count={count}", entries.Count);

        var newEntries = entries.Select(x => x.LogSequenceNumber).Except(hashLsn).ToArray();
        newEntries.ForEach(X => hashLsn.Add(X));

        var details = newEntries
            .Join(entries, x => x, x => x.LogSequenceNumber, (lsn, entry) => entry)
            .ToArray();

        details.ForEach(x => context.LogInformation(x.ToLoggingFormat()));
    }
}
