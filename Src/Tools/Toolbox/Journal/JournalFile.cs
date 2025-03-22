using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Logging;
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
    private ActionBlock<IReadOnlyList<JournalEntry>>? _writeBlock;

    public JournalFile(JournalFileOption fileOption, IFileStore fileStore, ILogger<JournalFile> logger)
    {
        _fileOption = fileOption.NotNull();
        _fileStore = fileStore.NotNull();
        _logger = logger.NotNull();

        var values = PropertyStringSchema.ConnectionString.Parse(_fileOption.ConnectionString).ThrowOnError().Return();
        _name = values.Single().Key.NotEmpty();
        _basePath = values.Single().Value.NotEmpty();

        _logger.LogInformation("JournalFile created, name={name}, basePath={basePath}", _name, _basePath);

        if (_fileOption.ReadOnly)
        {
            _logger.LogInformation("JournalFile is readonly, name={name}", _name);
            _writer = (_, _) => { return Task.FromResult(new Option(StatusCode.OK)); };
            return;
        }

        if (_fileOption.UseBackgroundWriter)
        {
            _logger.LogInformation("JournalFile using background writer, name={name}", _name);
            _writeBlock = new ActionBlock<IReadOnlyList<JournalEntry>>(async x => await InternalWrite(x, NullScopeContext.Default));
            _writer = QueueWrite;
            return;
        }

        _logger.LogInformation("JournalFile is setup, name={name}", _name);
        _writer = InternalWrite;
    }

    public async Task Close()
    {
        var writeBlock = Interlocked.Exchange(ref _writeBlock, null);
        if (writeBlock == null) return;

        writeBlock.Complete();
        await writeBlock.Completion;
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

        Option result;
        try
        {
            result = await _fileStore.File(path).Append(Encoding.UTF8.GetBytes(writeString), context);
        }
        finally
        {
            _writeLock.Release();
        }

        result.LogStatus(context, "Completed writting journal entry to name={name}, path={path}", [_name, path]);
        return result;
    }

    private async Task<Option> QueueWrite(IReadOnlyList<JournalEntry> journalEntries, ScopeContext context)
    {
        context.LogTrace("Queueing write journal entries, count={count}", journalEntries.Count);
        await _writeBlock.NotNull().SendAsync(journalEntries.ToArray(), context.CancellationToken);

        return StatusCode.OK;
    }
}
