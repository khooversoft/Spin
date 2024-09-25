using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.TransactionLog;

public static class TransactionLogTool
{
    public static IReadOnlyList<JournalEntry> ParseJournals(string data) => data.NotEmpty()
        .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
        .Select(x => x.ToObject<JournalEntry>() ?? throw new ArgumentException($"Failed to parse line: {x}"))
        .ToImmutableList();
}
