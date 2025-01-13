using System.Collections.Immutable;
using System.Text;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Journal;

public interface IJournalFile
{
    public IJournalTrx CreateTransactionContext(string? transactionId = null);
    Task<IReadOnlyList<JournalEntry>> ReadJournals(ScopeContext context);
    Task<Option> Write(IReadOnlyList<JournalEntry> journalEntries, ScopeContext context);

}

public class JournalFile : IJournalFile
{
    private readonly JournalFileOption _fileOption;
    private readonly ILogger<JournalFile> _logger;
    private readonly IFileStore _fileStore;
    private readonly string _name;
    private readonly string _basePath;
    private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);
    private readonly LogSequenceNumber _logSequenceNumber = new LogSequenceNumber();

    public JournalFile(JournalFileOption fileOption, IFileStore fileStore, ILogger<JournalFile> logger)
    {
        _fileOption = fileOption.NotNull();
        _fileStore = fileStore.NotNull();
        _logger = logger.NotNull();


        var values = PropertyStringSchema.ConnectionString.Parse(_fileOption.ConnectionString).ThrowOnError().Return();
        _name = values.Single().Key.NotEmpty();
        _basePath = values.Single().Value.NotEmpty();

        _logger.LogInformation("JournalFile created, name={name}, basePath={basePath}", _name, _basePath);
    }

    public IJournalTrx CreateTransactionContext(string? transactionId = null) => transactionId switch
    {
        not null => new JournalTrx(this, transactionId, _logger),
        null => new JournalTrx(this, Guid.NewGuid().ToString(), _logger)
    };

    public async Task<IReadOnlyList<JournalEntry>> ReadJournals(ScopeContext context)
    {
        var files = await _fileStore.Search($"{_basePath}/**/*.{_name}.json", context);

        var journalEntries = new Sequence<JournalEntry>();
        foreach (var file in files)
        {
            context.LogTrace("Reading journal file={file}", file);

            var readOption = await _fileStore.Get(file, context);
            if (readOption.IsError())
            {
                readOption.LogStatus(context, $"Reading file={file}");
                context.LogError("Error in reading journal file={file}", file);
                continue;
            }

            DataETag read = readOption.Return();
            string data = read.Data.BytesToString();

            var lines = data.NotEmpty()
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.ToObject<JournalEntry>())
                .OfType<JournalEntry>()
                .ToArray();

            journalEntries += lines;
        }

        return journalEntries.ToImmutableArray();
    }

    public async Task<Option> Write(IReadOnlyList<JournalEntry> journalEntries, ScopeContext context)
    {
        journalEntries.NotNull();
        context = context.With(_logger);

        var writeString = journalEntries
            .Select(x => x.LogSequenceNumber.IsNotEmpty() ? x : x with { LogSequenceNumber = _logSequenceNumber.Next() })
            .Select(x => x.ToJson() + Environment.NewLine)
            .Aggregate(new StringBuilder(), (a, b) => a.Append(b))
            .ToString();

        string path = $"{_basePath}/{DateTime.UtcNow:yyyyMM}/{DateTime.UtcNow:yyyyMMdd}.{_name}.json";

        string logSequenceNumbers = journalEntries.Select(x => x.LogSequenceNumber).Join(",");
        context.LogTrace("Writting journal entry to name={name}, path={path}, lsns={lsns}", _name, path, logSequenceNumbers);

        await _writeLock.WaitAsync(context.CancellationToken);

        Option result;
        try
        {
            result = await _fileStore.Append(path, Encoding.UTF8.GetBytes(writeString), context);
        }
        finally
        {
            _writeLock.Release();
        }

        result.LogStatus(context, "Completed writting journal entry to name={name}, path={path}", [_name, path]);
        return result;
    }
}
