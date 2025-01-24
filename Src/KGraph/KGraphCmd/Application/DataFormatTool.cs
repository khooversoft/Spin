using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Journal;
using Toolbox.Tools;
using Toolbox.Types;

namespace KGraphCmd.Application;

internal static class DataFormatTool
{
    public static DataFormatCollection Formats { get; } = new DataFormatCollection()
    {
        new DataFormat(typeof(GraphNode), DataFormatType.Single, x => Format(x, DataFormatType.Single)),
        new DataFormat(typeof(GraphNode), DataFormatType.Full, x => Format(x, DataFormatType.Full)),

        new DataFormat(typeof(GraphEdge), DataFormatType.Single, x => Format(x, DataFormatType.Single)),
        new DataFormat(typeof(GraphEdge), DataFormatType.Full, x => Format(x, DataFormatType.Full)),

        new DataFormat(typeof(QueryBatchResult), DataFormatType.Single, x => Format(x, DataFormatType.Single)),
        new DataFormat(typeof(QueryBatchResult), DataFormatType.Full, x => Format(x, DataFormatType.Full)),

        new DataFormat(typeof(QueryResult), DataFormatType.Single, x => Format(x, DataFormatType.Single)),
        new DataFormat(typeof(QueryResult), DataFormatType.Full, x => Format(x, DataFormatType.Full)),

        new DataFormat(typeof(JournalEntry), DataFormatType.Single, x => Format(x, DataFormatType.Single)),
        new DataFormat(typeof(JournalEntry), DataFormatType.Full, x => Format(x, DataFormatType.Full)),

        new DataFormat(typeof(GraphLinkData), DataFormatType.Single, x => Format(x, DataFormatType.Single)),
    };

    public static IReadOnlyList<KeyValuePair<string, string?>> Format<T>(this T subject, DataFormatType formatType) where T : class
    {
        subject.NotNull();

        IReadOnlyList<KeyValuePair<string, string?>> single = subject switch
        {
            GraphNode v => FormatSingle(v),
            GraphEdge v => FormatSingle(v),
            QueryBatchResult v => FormatSingle(v),
            QueryResult v => FormatSingle(v),
            JournalEntry v => FormatSingle(v),
            GraphLinkData v => FormatSingle(v),

            _ => throw new ArgumentException($"Unknown subject type={subject.GetType().Name}"),
        };

        IEnumerable<KeyValuePair<string, string?>> result = formatType switch
        {
            DataFormatType.Single => single,
            DataFormatType.Full => single.Concat(subject.ToKeyValuePairs()),

            _ => throw new ArgumentException($"Unknown format type={formatType.ToString()}"),
        };

        return result.ToImmutableArray();
    }

    public static IReadOnlyList<KeyValuePair<string, string?>> FormatSingle(this GraphNode subject) => [new KeyValuePair<string, string?>("Node", $"Key={subject.Key}")];
    public static IReadOnlyList<KeyValuePair<string, string?>> FormatSingle(this GraphEdge subject) => [new KeyValuePair<string, string?>("Edge", subject.ToString())];

    public static IReadOnlyList<KeyValuePair<string, string?>> FormatSingle(this JournalEntry subject) =>
    [
        new KeyValuePair<string, string?>("Journal", $"Lsn={subject.LogSequenceNumber}, Date={subject.Date:o}, Type={subject.Type}, {subject.GetEntryKey()}")
    ];

    public static IReadOnlyList<KeyValuePair<string, string?>> FormatSingle(this QueryResult subject) =>
    [
        new KeyValuePair<string, string?>("QueryResult", $"Option={subject.Option}, Nodes={subject.Nodes.Count}, Edges={subject.Edges.Count}"),
        .. subject.Nodes.SelectMany(x => x.FormatSingle()),
        .. subject.Edges.SelectMany(x => x.FormatSingle()),
    ];

    public static IReadOnlyList<KeyValuePair<string, string?>> FormatSingle(this QueryBatchResult subject) =>
    [
        new KeyValuePair<string, string?>("QueryBatchResult", $"Option={subject.Option}, Items={subject.Items.Count}"),
        .. subject.Items.SelectMany(x => x.FormatSingle()),
    ];

    public static IReadOnlyList<KeyValuePair<string, string?>> FormatSingle(this GraphLinkData subject) =>
    [
        new KeyValuePair<string, string?>("GraphLinkData", $"NodeKey={subject.NodeKey}, Name={subject.Name}, FileId={subject.FileId}"),
        new KeyValuePair<string, string?>("GraphLinkData", $"Data={subject.Data.ToJsonFromData()}"),
    ];

    private static string GetEntryKey(this JournalEntry subject) => subject switch
    {
        var v => v.Type switch
        {
            JournalType.Start => "Start transactions",
            JournalType.Commit => "Commit transactions",
            JournalType.Action when subject.Data.TryGetValue("Command", out var commandValue) => $"Command={commandValue.Truncate(50)}".GetStatusAndType(subject),
            JournalType.Data when subject.Data.TryGetValue("GraphQuery", out var graphQueryValue) => $"GraphQuery={graphQueryValue.Truncate(50)}".GetStatusAndType(subject),

            _ => "data...".GetStatusAndType(subject),
        },
    };

    private static string GetStatusAndType(this string description, JournalEntry subject)
    {
        var type = subject.Data.TryGetValue("$type", out var typeValue) ? typeValue : null;
        string? statusCode = search("StatusCode");
        string? error = search("Error");

        var result = new[]
        {
            description,
            type != null ? $"$type={type}" : null,
            statusCode != null ? $"statusCode={statusCode}" : null,
            error != null ? $"error={error}" : null,
        }.Join(", ");

        return result;

        string? search(string key) => subject.Data.TryGetValue(key, out var value) switch
        {
            true => value,

            false => subject.Data
                .Where(x => x.Key.IndexOf(key, 0, StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(x => x.Value)
                .FirstOrDefault(),
        };
    }
}
