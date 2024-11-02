using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.TransactionLog;

public interface ITransactionLogWriter
{
    public string Name { get; }
    Task<Option> Write(JournalEntry journalEntry, ScopeContext context);
    Task<IReadOnlyList<JournalEntry>> ReadJournals(ScopeContext context);
}

public class TransactionLogFile : ITransactionLogWriter, IAsyncDisposable
{
    private readonly IFileStore _fileStore;
    private readonly string _basePath;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _resetEvent = new SemaphoreSlim(1, 1);
    private readonly TransactionLogFileOption _transactionLogFileOption;
    private int _journalNumber = 0;
    private Writer _writer;

    public TransactionLogFile(TransactionLogFileOption transactionLogFileOption, IFileStore fileStore, ILogger<TransactionLogFile> logger)
    {
        _transactionLogFileOption = transactionLogFileOption;
        _fileStore = fileStore.NotNull();
        _logger = logger.NotNull();

        var values = PropertyStringSchema.ConnectionString.Parse(_transactionLogFileOption.ConnectionString).ThrowOnError().Return();
        Name = values.Single().Key.NotEmpty();
        _basePath = values.Single().Value.NotEmpty();

        _writer = new Writer(_fileStore, Name, _basePath, _journalNumber);
    }

    public string Name { get; }

    public async Task<Option> Write(JournalEntry journalEntry, ScopeContext context)
    {
        context = context.With(_logger);
        await _resetEvent.WaitAsync(context.CancellationToken);

        try
        {
            var result = await _writer.Write(journalEntry, context);
            if (result.IsError()) return result;

            Interlocked.Increment(ref _journalNumber);
            if (_writer.Count > _transactionLogFileOption.MaxCount)
            {
                context.LogInformation("Closing writer due to max count reached name={name}, count={count}", Name, _writer.Count);
                _writer = new Writer(_fileStore, Name, _basePath, _journalNumber);
            }

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

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private class Writer
    {
        private readonly IFileStore _fileStore;
        private readonly string _name;
        private readonly string _path;
        private readonly string _basePath;
        private readonly int _journalNumber;
        private int _count = 0;

        public Writer(IFileStore fileStore, string name, string basePath, int journalNumber)
        {
            _fileStore = fileStore.NotNull();
            _name = name.NotEmpty();
            _basePath = basePath.NotEmpty();
            _journalNumber = journalNumber;

            string randString = RandomNumberGenerator.GetBytes(2).Func(x => BitConverter.ToUInt16(x, 0).ToString("X4"));

            _path = $"{_basePath}/{DateTime.UtcNow:yyyyMM}/{DateTime.UtcNow:yyyyMMdd-HH}-{_journalNumber:d04}-{randString}.tranLog.json";
        }

        public int Count => _count;

        public async Task<Option> Write(JournalEntry journalEntry, ScopeContext context)
        {
            context.LogInformation("Writting journal entry to name={name}, path={path}, lsn={lsn}", _name, _path, journalEntry.LogSequenceNumber);

            string jsonData = journalEntry.ToJson() + Environment.NewLine;
            var result = await _fileStore.Append(_path, Encoding.UTF8.GetBytes(jsonData), context);
            if (result.IsOk()) Interlocked.Increment(ref _count);

            result.LogStatus(context, "Completed writting journal entry to name={name}, path={path}", [_name, _path]);
            return result;
        }
    }
}
