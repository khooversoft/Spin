using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.TransactionLog;

// Log format
//
//  rootpath/translogName/yyyymm/yyyymmdd-hh.tranLog.json
//
// Connection string = {name}={basePath}
//

public record TransactionLogFileOption
{
    public string ConnectionString { get; init; } = null!;
    public int MaxCount { get; init; } = 1000;
}

public class TransactionLogFile : ITransactionLogWriter, IAsyncDisposable
{
    private readonly IFileStore _fileStore;
    private readonly string _basePath;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _resetEvent = new SemaphoreSlim(1, 1);
    private readonly TransactionLogFileOption _transactionLogFileOption;
    private int _journalNumber = 0;
    private Writer? _writer;

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

    public async Task<Option> Write(JournalEntry journalEntry, ScopeContext context)
    {
        context = context.With(_logger);
        _writer ??= new Writer(_fileStore, Name, _basePath);

        await _resetEvent.WaitAsync(context.CancellationToken);

        try
        {
            var result = await _writer.Write(journalEntry, context);
            if (result.IsError()) return result;

            Interlocked.Increment(ref _journalNumber);
            if (_writer.Count > _transactionLogFileOption.MaxCount)
            {
                context.LogInformation("Closing writer due to max count reached name={name}, count={count}", Name, _writer.Count);
                _writer = null;
            }

            return result;
        }
        finally
        {
            _resetEvent.Release();
        }
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private class Writer
    {
        private readonly IFileStore _fileStore;
        private readonly string _name;
        private readonly string _path;
        private readonly string _basePath;
        private int _count = 0;

        public Writer(IFileStore fileStore, string name, string basePath)
        {
            _fileStore = fileStore.NotNull();
            _name = name.NotEmpty();
            _basePath = basePath.NotEmpty();

            byte[] four_bytes = RandomNumberGenerator.GetBytes(4);
            string randString = BitConverter.ToUInt32(four_bytes, 0).ToString("X8");

            _path = $"{_basePath}/{DateTime.UtcNow:yyyyMM}/{DateTime.UtcNow:yyyyMMdd-HH}-{randString}.tranLog.json";
        }

        public int Count => _count;

        public async Task<Option> Write(JournalEntry journalEntry, ScopeContext context)
        {
            context.LogInformation("Writting journal entry to name={name}, path={path}, lsn={lsn}", _name, _path, journalEntry.LogSequenceNumber);

            string jsonData = journalEntry.ToJson() + Environment.NewLine;
            var result = await _fileStore.Append(_path, Encoding.UTF8.GetBytes(jsonData), context);
            if (result.IsOk()) Interlocked.Increment(ref _count);

            result.LogStatus(context, "Completed writting journal entry to name={name}, path={path}", _name, _path);
            return result;
        }
    }
}


