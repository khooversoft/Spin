using System.Text;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Journal;
using Toolbox.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.TransactionLog;

public interface ITransactionLogWriter
{
    public string Name { get; }
    Task<Option> Write(IReadOnlyList<JournalEntry> journalEntries, ScopeContext context);
    Task<IReadOnlyList<JournalEntry>> ReadJournals(ScopeContext context);
}

public class TransactionLogFile : ITransactionLogWriter
{
    private readonly IFileStore _fileStore;
    private readonly string _basePath;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _resetEvent = new SemaphoreSlim(1, 1);
    private readonly TransactionLogFileOption _transactionLogFileOption;

    public TransactionLogFile(TransactionLogFileOption transactionLogFileOption, IFileStore fileStore, ILogger<TransactionLogFile> logger)
    {
        _transactionLogFileOption = transactionLogFileOption;
        _fileStore = fileStore.NotNull();
        _logger = logger.NotNull();

        var values = PropertyStringSchema.ConnectionString.Parse(_transactionLogFileOption.ConnectionString).ThrowOnError().Return();
        Name = values.Single().Key.NotEmpty();
        _basePath = values.Single().Value.NotEmpty();
    }

    public string Name { get; }

    public async Task<Option> Write(IReadOnlyList<JournalEntry> journalEntries, ScopeContext context)
    {
        journalEntries.NotNull();
        context = context.With(_logger);

        await _resetEvent.WaitAsync(context.CancellationToken);

        try
        {
            string path = $"{_basePath}/{DateTime.UtcNow:yyyyMM}/{DateTime.UtcNow:yyyyMMdd}.tranLog.json";

            string logSequenceNumbers = journalEntries.Select(x => x.LogSequenceNumber).Join(",");
            context.LogInformation("Writting journal entry to name={name}, path={path}, lsns={lsns}", Name, path, logSequenceNumbers);

            var json = journalEntries
                .Select(x => x.ToJson() + Environment.NewLine)
                .Aggregate(new StringBuilder(), (a, b) => a.Append(b))
                .ToString();

            var result = await _fileStore.Append(path, Encoding.UTF8.GetBytes(json), context);

            result.LogStatus(context, "Completed writting journal entry to name={name}, path={path}", [Name, path]);
            return result;
        }
        finally
        {
            _resetEvent.Release();
        }
    }

    public async Task<IReadOnlyList<JournalEntry>> ReadJournals(ScopeContext context)
    {
        var result = await TransactionLogTool.ReadAndParseJournals(_fileStore, _basePath, context);
        return result;
    }
}
