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

internal class TraceLog : ICommandRoute
{
    private readonly AbortSignal _abortSignal;
    private readonly ILogger<TraceLog> _logger;
    private readonly ScopeContext _context;
    private readonly GraphHostManager _graphHostManager;

    public TraceLog(GraphHostManager graphHostManager, AbortSignal abortSignal, ILogger<TraceLog> logger)
    {
        _graphHostManager = graphHostManager.NotNull();
        _abortSignal = abortSignal.NotNull();
        _logger = logger.NotNull();
        _context = new ScopeContext(_logger);
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("trace", "Dump or reset KGraph's database traces")
    {
        new CommandSymbol("list", "List traces").Action(x =>
        {
            var jsonFile = x.AddOption<string?>("--config", "Json file with data lake connection details");
            var monitor = x.AddOption<bool>("--monitor", "Monitor traces");
            x.SetHandler(List, jsonFile, monitor);
        }),
    };

    private async Task List(string? jsonFile, bool monitor)
    {
        if (jsonFile.IsNotEmpty()) _graphHostManager.Start(jsonFile);
        _context.LogInformation("Starting to list traces...");

        var traceLog = _graphHostManager.ServiceProvider.GetRequiredKeyedService<IJournalFile>(GraphConstants.Trace.DiKeyed).NotNull();
        var hashLsn = new HashSet<string>();

        while (!_abortSignal.GetToken().IsCancellationRequested)
        {
            await ReadJournal(traceLog, hashLsn, _context);
            if (!monitor) return;

            await InputTool.WaitForInput(_abortSignal.GetToken());

            var match = InputTool.GetUserCommand(_abortSignal.GetToken(), "continue", "quit", "reset");
            switch (match)
            {
                case "continue": continue;
                case "quit": return;

                case "reset":
                    hashLsn.Clear();
                    continue;
            }
        }
    }

    private async Task ReadJournal(IJournalFile journalFile, HashSet<string> hashLsn, ScopeContext context)
    {
        IReadOnlyList<JournalEntry> entries = await journalFile.ReadJournals(context);
        context.LogTrace("Readed journals, count={count}", entries.Count);

        var newEntries = entries.Select(x => x.LogSequenceNumber).Except(hashLsn).ToArray();
        newEntries.ForEach(X => hashLsn.Add(X));

        var details = newEntries
            .Join(entries, x => x, x => x.LogSequenceNumber, (lsn, entry) => entry)
            .ToArray();

        details.ForEach(x => context.LogInformation(x.ToLoggingFormat()));
    }
}
