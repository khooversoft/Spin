using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.TransactionLog;

public static class TransactionLogTool
{
    public static IReadOnlyList<JournalEntry> ParseJournals(string data) => data.NotEmpty()
        .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
        .Select(x => x.ToObject<JournalEntry>() ?? throw new ArgumentException($"Failed to parse line: {x}"))
        .ToImmutableList();

    public static async Task<IReadOnlyList<JournalEntry>> ReadAndParseJournals(IFileStore fileStore, string journalPath, ScopeContext context)
    {
        fileStore.NotNull();
        journalPath.NotEmpty();

        var files = await fileStore.Search($"{journalPath}/**/*.tranLog.json", context);

        var journalEntries = new Sequence<JournalEntry>();
        foreach (var file in files)
        {
            context.LogInformation("Reading journal file={file}", file);

            var readOption = await fileStore.Get(file, context);
            if (readOption.IsError())
            {
                readOption.LogStatus(context, $"Reading file={file}");
                context.LogError("Error in reading journal file={file}", file);
                continue;
            }

            DataETag read = readOption.Return();
            string data = read.Data.BytesToString();

            journalEntries += ParseJournals(data);
        }

        return journalEntries.ToImmutableArray();
    }
}
