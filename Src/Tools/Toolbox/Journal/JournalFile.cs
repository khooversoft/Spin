using System.Collections.Immutable;
using System.Text;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Journal;

public interface IJournalFile : IAsyncDisposable
{
    Task Close();
    IJournalTrx CreateTransactionContext(string? transactionId = null);
    Task<IReadOnlyList<string>> GetFiles(ScopeContext context);
    Task<IReadOnlyList<JournalEntry>> ReadJournals(ScopeContext context);
    Task<Option> Write(IReadOnlyList<JournalEntry> journalEntries, ScopeContext context);

}

public class JournalFile : IJournalFile, IAsyncDisposable
{
    private readonly JournalFileOption _fileOption;
    private readonly ILogger<JournalFile> _logger;
    private readonly IFileStore _fileStore;
    private readonly string _name;
    private readonly string _basePath;
    private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);
    private readonly LogSequenceNumber _logSequenceNumber = new LogSequenceNumber();
    private readonly Func<IReadOnlyList<JournalEntry>, ScopeContext, Task<Option>> _writer;
    private AutoFlushQueue<JournalEntry>? _autoFlushQueue;
    private readonly ScopeContext _context;

    public JournalFile(JournalFileOption fileOption, IFileStore fileStore, ILogger<JournalFile> logger)
    {
        _fileOption = fileOption.NotNull();
        _fileStore = fileStore.NotNull();
        _logger = logger.NotNull();

        var values = PropertyStringSchema.ConnectionString.Parse(_fileOption.ConnectionString).ThrowOnError().Return();
        _name = values.Single().Key.NotEmpty();
        _basePath = values.Single().Value.NotEmpty();

        _logger.LogTrace("JournalFile created, name={name}, basePath={basePath}", _name, _basePath);

        if (_fileOption.ReadOnly)
        {
            _logger.LogInformation("JournalFile is readonly, name={name}", _name);
            _writer = (_, _) => { return Task.FromResult(new Option(StatusCode.OK)); };
            return;
        }

        if (_fileOption.UseBackgroundWriter)
        {
            _logger.LogInformation("JournalFile using background writer, name={name}", _name);
            _context = new ScopeContext(_logger);
            _autoFlushQueue = new AutoFlushQueue<JournalEntry>(1000, TimeSpan.FromMilliseconds(100), async x => await FlushQueue(x, _context));
            _writer = QueueWrite;
            return;
        }

        _logger.LogTrace("JournalFile is setup, name={name}", _name);
        _writer = InternalWrite;
    }

    public async Task Close()
    {
        var autoFlushQueue = Interlocked.Exchange(ref _autoFlushQueue, null);
        if (autoFlushQueue == null) return;

        await autoFlushQueue.Complete();
    }

    public IJournalTrx CreateTransactionContext(string? transactionId = null) => transactionId switch
    {
        not null => new JournalTrx(this, transactionId, _logger),
        null => new JournalTrx(this, Guid.NewGuid().ToString(), _logger)
    };

    public async ValueTask DisposeAsync() => await Close();

    public async Task<IReadOnlyList<string>> GetFiles(ScopeContext context) =>
        (await _fileStore.Search($"{_basePath}/**/*.{_name}.json", context))
        .Select(x => x.Path)
        .ToImmutableArray();


    public async Task<IReadOnlyList<JournalEntry>> ReadJournals(ScopeContext context)
    {
        var files = await GetFiles(context);

        var journalEntries = new Sequence<JournalEntry>();
        foreach (var file in files)
        {
            context.LogTrace("Reading journal file={file}", file);

            Option<DataETag> readOption = await _fileStore.File(file).Get(context);
            if (readOption.IsError())
            {
                readOption.LogStatus(context, $"Reading file={file}");
                context.LogError("Error in reading journal file={file}", file);
                continue;
            }

            DataETag read = readOption.Return();
            string data = read.Data.BytesToString();
            if (data.IsEmpty()) continue;

            var lines = data
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.ToObject<JournalEntry>())
                .OfType<JournalEntry>()
                .ToArray();

            journalEntries += lines;
        }

        return journalEntries.ToImmutableArray();
    }

    public Task<Option> Write(IReadOnlyList<JournalEntry> journalEntries, ScopeContext context) => _writer(journalEntries, context);


    private async Task<Option> InternalWrite(IReadOnlyList<JournalEntry> journalEntries, ScopeContext context)
    {
        journalEntries.NotNull();
        context = context.With(_logger);
        _fileOption.ReadOnly.Assert(x => x == false, "Cannot write map when read-only");

        var writeString = journalEntries
            .Select(x => x.LogSequenceNumber.IsNotEmpty() ? x : x with { LogSequenceNumber = _logSequenceNumber.Next() })
            .Select(x => x.ToJson() + Environment.NewLine)
            .Aggregate(new StringBuilder(), (a, b) => a.Append(b))
            .ToString();

        string path = $"{_basePath}/{DateTime.UtcNow:yyyyMM}/{DateTime.UtcNow:yyyyMMdd}.{_name}.json";

        string logSequenceNumbers = journalEntries.Select(x => x.LogSequenceNumber).Join(",");
        context.LogTrace("Writting journal entry to name={name}, path={path}, lsns={lsns}", _name, path, logSequenceNumbers);

        await _writeLock.WaitAsync(context.CancellationToken);

        Option<string> result;
        try
        {
            result = await _fileStore.File(path).Append(Encoding.UTF8.GetBytes(writeString), context);
        }
        finally
        {
            _writeLock.Release();
        }

        result.LogStatus(context, "Completed writting journal entry to name={name}, path={path}", [_name, path]);
        return result.ToOptionStatus();
    }

    private async Task<Option> QueueWrite(IReadOnlyList<JournalEntry> journalEntries, ScopeContext context)
    {
        context.LogTrace("Queueing write journal entries, count={count}", journalEntries.Count);
        await _autoFlushQueue.NotNull().Enqueue(journalEntries, context);

        return StatusCode.OK;
    }

    private async Task FlushQueue(IReadOnlyList<JournalEntry> journalEntries, ScopeContext context)
    {
        context.LogTrace("Flushing queue journal entries, count={count}", journalEntries.Count);
        await InternalWrite(journalEntries, context);
    }
}
