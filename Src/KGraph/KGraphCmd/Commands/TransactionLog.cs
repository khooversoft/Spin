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

    public TransactionLog(AbortSignal abortSignal, ILogger<TraceLog> logger)
    {
        _abortSignal = abortSignal.NotNull();
        _logger = logger.NotNull();
        _context = new ScopeContext(_logger);
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("trx", "Dump or reset KGraph's database transaction logs")
    {
        new CommandSymbol("list", "List transactions").Action(x =>
        {
            var jsonFile = x.AddArgument<string>("jsonFile", "Json file with data lake connection details");
            x.SetHandler(List, jsonFile);
        }),
        new CommandSymbol("clear", "Clear transactions").Action(x =>
        {
            var jsonFile = x.AddArgument<string>("jsonFile", "Json file with data lake connection details");
            x.SetHandler(Clear, jsonFile);
        }),
    };

    private async Task List(string jsonFile)
    {
        await using var services = HostTool.StartHost(jsonFile);

        var traceLog = services.GetRequiredKeyedService<IJournalFile>(GraphConstants.TrxJournal.DiKeyed).NotNull();
        var hashLsn = new HashSet<string>();

        while (!_abortSignal.GetToken().IsCancellationRequested)
        {
            await ReadJournal(traceLog, hashLsn, _context);
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    private async Task Clear(string jsonFile)
    {
        await using var services = HostTool.StartHost(jsonFile);

        var fileStore = services.GetRequiredService<IFileStore>().NotNull();

        var traceLog = services.GetRequiredKeyedService<IJournalFile>(GraphConstants.TrxJournal.DiKeyed).NotNull();
        var files = await traceLog.GetFiles(_context);

        foreach (var file in files)
        {
            _context.LogInformation("Deleting file {file}", file);
            var option = await fileStore.Delete(file, _context);
            if (option.IsError()) option.LogStatus(_context, "Failed to delete file {file}", [file]);
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
