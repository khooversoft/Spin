using System.Collections.Immutable;
using System.Text;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Journal;

public interface IJournalWriter
{
    Task<Option> Write(IReadOnlyList<JournalEntry> journalEntries, ScopeContext context);
    Task<IReadOnlyList<JournalEntry>> ReadJournals(ScopeContext context);
}

public class JournalFile
{
    private readonly JournalFileOption _fileOption;
    private readonly ILogger<JournalFile> _logger;
    private readonly IFileStore _fileStore;
    private readonly string _name;
    private readonly string _basePath;
    private readonly SemaphoreSlim _resetEvent = new SemaphoreSlim(1, 1);

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

    public async Task<IReadOnlyList<JournalEntry>> ReadJournals(ScopeContext context)
    {
        var files = await _fileStore.Search($"{_basePath}/**/*.{_name}.json", context);

        var journalEntries = new Sequence<JournalEntry>();
        foreach (var file in files)
        {
            context.LogInformation("Reading journal file={file}", file);

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

        await _resetEvent.WaitAsync(context.CancellationToken);

        try
        {
            string path = $"{_basePath}/{DateTime.UtcNow:yyyyMM}/{DateTime.UtcNow:yyyyMMdd}.{_name}.json";

            string logSequenceNumbers = journalEntries.Select(x => x.LogSequenceNumber).Join(",");
            context.LogInformation("Writting journal entry to name={name}, path={path}, lsns={lsns}", _name, path, logSequenceNumbers);

            var json = journalEntries
                .Select(x => x.ToJson() + Environment.NewLine)
                .Aggregate(new StringBuilder(), (a, b) => a.Append(b))
                .ToString();

            var result = await _fileStore.Append(path, Encoding.UTF8.GetBytes(json), context);

            result.LogStatus(context, "Completed writting journal entry to name={name}, path={path}", [_name, path]);
            return result;
        }
        finally
        {
            _resetEvent.Release();
        }
    }
}
