using System.Collections;
using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Journal;
using Toolbox.Tools;
using Toolbox.Types;

namespace KGraphCmd.Application;

internal class DataFormatCollection : IEnumerable<DataFormat>
{
    private readonly List<DataFormat> _dataFormats = new List<DataFormat>();
    public void Add(DataFormat dataFormat) => _dataFormats.Add(dataFormat);

    public IReadOnlyList<KeyValuePair<string, string?>> SingleFormat<T>(T subject) where T : class => Format<T>(subject, DataFormatType.Single);
    public IReadOnlyList<KeyValuePair<string, string?>> FullFormat<T>(T subject) where T : class => Format<T>(subject, DataFormatType.Full);

    public IReadOnlyList<KeyValuePair<string, string?>> Format<T>(T subject, DataFormatType formatType) where T : class
    {
        var formatEntry = _dataFormats.FirstOrDefault(x => typeof(T) == x.SubjectType && x.FormatType == formatType);
        if (formatEntry == null) throw new ArgumentException($"No format found for {typeof(T).Name} and FormatType={formatType}");
        return formatEntry.Format(subject);
    }

    public IEnumerator<DataFormat> GetEnumerator() => ((IEnumerable<DataFormat>)_dataFormats).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dataFormats).GetEnumerator();
}

public enum DataFormatType
{
    Single,
    Full
}

internal class DataFormat
{
    public DataFormat(Type type, DataFormatType formatType, Func<object, IReadOnlyList<KeyValuePair<string, string?>>> format)
    {
        SubjectType = type;
        FormatType = formatType;
        Format = format;
    }

    public Type SubjectType { get; }
    public DataFormatType FormatType { get; }
    public Func<object, IReadOnlyList<KeyValuePair<string, string?>>> Format { get; }
}

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

    private static string GetEntryKey(this JournalEntry subject) => subject.Data.TryGetValue("$type", out var typeValue) switch
    {
        true => typeValue switch
        {
            "GraphTrace" => GetGraphTraceSingle(subject),
            "QueryBatchResult" => GetQueryBatchResultSingle(subject),

            _ => throw new ArgumentException($"Unknown typeValue={typeValue}"),
        },

        _ => "No $type",
    };

    private static string GetGraphTraceSingle(this JournalEntry subject)
    {
        var type = subject.Data.TryGetValue("$type", out var typeValue) ? typeValue : "Unknown";
        var command = subject.Data.TryGetValue("Command", out var commandValue) ? commandValue.Truncate(30) : "Unknown";

        return $"$type={type}, command={command}";
    }

    private static string GetQueryBatchResultSingle(this JournalEntry subject)
    {
        var type = subject.Data.TryGetValue("$type", out var typeValue) ? typeValue : "Unknown";
        var statusCode = subject.Data.TryGetValue("StatusCode", out var statusCodeValue) ? statusCodeValue : "Unknown";

        return $"$type={type}, statusCode={statusCode}";
    }
}
