using Toolbox.Types;

namespace Toolbox.Data;

public interface IJournalFile : IAsyncDisposable
{
    Task Close(ScopeContext context);
    IJournalTrx CreateTransactionContext(string? transactionId = null);
    Task<IReadOnlyList<string>> GetFiles(ScopeContext context);
    Task<IReadOnlyList<JournalEntry>> ReadJournals(ScopeContext context);
    Task<Option> Write(IReadOnlyList<JournalEntry> journalEntries, ScopeContext context);
}



//public class JournalFile : IJournalFile, IAsyncDisposable
//{
//    private readonly ILogger<JournalFile> _logger;
//    private readonly string _basePath;
//    private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);
//    private readonly LogSequenceNumber _logSequenceNumber = new LogSequenceNumber();
//    //private readonly Func<IReadOnlyList<JournalEntry>, ScopeContext, Task<Option>> _writer;
//    private readonly ScopeContext _context;
//    private readonly IDataClient<JournalEntry> _dataClient;

//    public JournalFile(IDataClient<JournalEntry> dataClient, ILogger<JournalFile> logger)
//    {
//        _dataClient = dataClient.NotNull();
//        _logger = logger.NotNull();

//        _logger.LogDebug("JournalFile is setup");
//        _writer = InternalWrite;
//    }

//    public async Task Close(ScopeContext context)
//    {
//        var autoFlushQueue = Interlocked.Exchange(ref _autoFlushQueue, null);
//        if (autoFlushQueue == null) return;

//        await autoFlushQueue.Complete(context);
//        _logger.ToScopeContext().LogDebug("Closed journal file={name}", _name);
//    }

//    public IJournalTrx CreateTransactionContext(string? transactionId = null) => transactionId switch
//    {
//        not null => new JournalTrx(this, transactionId, _logger),
//        null => new JournalTrx(this, Guid.NewGuid().ToString(), _logger)
//    };

//    public async ValueTask DisposeAsync() => await Close(NullScopeContext.Instance);

//    public async Task<IReadOnlyList<string>> GetFiles(ScopeContext context) =>
//        (await _fileStore.Search($"{_basePath}/**/*.{_name}.json", context))
//        .Select(x => x.Path)
//        .ToImmutableArray();


//    public async Task<IReadOnlyList<JournalEntry>> ReadJournals(ScopeContext context)
//    {
//        var files = await GetFiles(context);

//        var journalEntries = new Sequence<JournalEntry>();
//        foreach (var file in files)
//        {
//            context.LogDebug("Reading journal file={file}", file);

//            Option<DataETag> readOption = await _fileStore.File(file).Get(context);
//            if (readOption.IsError())
//            {
//                readOption.LogStatus(context, $"Reading file={file}");
//                context.LogError("Error in reading journal file={file}", file);
//                continue;
//            }

//            DataETag read = readOption.Return();
//            string data = read.Data.BytesToString();
//            if (data.IsEmpty()) continue;

//            var lines = data
//                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
//                .Select(x => x.ToObject<JournalEntry>())
//                .OfType<JournalEntry>()
//                .ToArray();

//            journalEntries += lines;
//        }

//        return journalEntries.ToImmutableArray();
//    }

//    public Task<Option> Write(IReadOnlyList<JournalEntry> journalEntries, ScopeContext context) => _writer(journalEntries, context);


//    private async Task<Option> InternalWrite(IReadOnlyList<JournalEntry> journalEntries, ScopeContext context)
//    {
//        journalEntries.NotNull();
//        context = context.With(_logger);

//        var writeString = journalEntries
//            .Select(x => x.LogSequenceNumber.IsNotEmpty() ? x : x with { LogSequenceNumber = _logSequenceNumber.Next() })
//            .Select(x => x.ToJson() + Environment.NewLine)
//            .Aggregate(new StringBuilder(), (a, b) => a.Append(b))
//            .ToString();

//        string path = $"{_basePath}/{DateTime.UtcNow:yyyyMM}/{DateTime.UtcNow:yyyyMMdd}.{_name}.json";

//        string logSequenceNumbers = journalEntries.Select(x => x.LogSequenceNumber).Join(",");
//        context.LogDebug("Writing journal entry to name={name}, path={path}, lsns={lsns}", _name, path, logSequenceNumbers);

//        await _writeLock.WaitAsync(context.CancellationToken);

//        Option<string> result;
//        try
//        {
//            result = await _fileStore.File(path).Append(Encoding.UTF8.GetBytes(writeString), context);
//        }
//        finally
//        {
//            _writeLock.Release();
//        }

//        result.LogStatus(context, "Completed writing journal entry to name={name}, path={path}", [_name, path]);
//        return result.ToOptionStatus();
//    }

//    private async Task<Option> QueueWrite(IReadOnlyList<JournalEntry> journalEntries, ScopeContext context)
//    {
//        context.LogDebug("Queueing write journal entries, count={count}", journalEntries.Count);
//        await _autoFlushQueue.NotNull().Enqueue(journalEntries, context);

//        return StatusCode.OK;
//    }

//    private async Task FlushQueue(IReadOnlyList<JournalEntry> journalEntries, ScopeContext context)
//    {
//        context.LogDebug("Flushing queue journal entries, count={count}", journalEntries.Count);
//        await InternalWrite(journalEntries, context);
//    }
//}
