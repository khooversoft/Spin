using Toolbox.Tools;

namespace Toolbox.Data;

public record TableRow
{
    public TableRow(IEnumerable<object?> dataItems, TableHeader header, string? tag, string? key)
    {
        dataItems.NotNull();
        Header = header.NotNull();

        Tag = tag;
        Key = key;

        Items = dataItems
            .Select((x, i) => new DataItem(i, x))
            .ToArray();
    }

    public TableHeader Header { get; }

    public object this[int index] => Items[index];

    public IReadOnlyList<DataItem> Items { get; }
    public string? Tag { get; }
    public string? Key { get; }

    public T Get<T>(int index) => Items[index].Get<T>();
    public T Get<T>(string name) => Items[Header.NotNull().ByName[name]].Get<T>();
}


public record DataItem
{
    public DataItem(int index, object? value) => (Index, Value) = (index, value);

    public int Index { get; }
    public object? Value { get; }

    public T Get<T>()
    {
        if (Value == null) return default!;

        return typeof(T) switch
        {
            Type v when v == typeof(string) => (T)(object)toObjectValue(Value),
            Type v when v == typeof(int) => (T)Convert.ChangeType(Value, typeof(int)),
            Type v when v == typeof(long) => (T)Convert.ChangeType(Value, typeof(long)),
            Type v when v == typeof(DateTime) => (T)(object)DateTime.Parse(Value.ToString()!),

            _ => throw new ArgumentException($"Unsupported data type, type={typeof(T).FullName}"),
        };

        string toObjectValue(object? value) => value?.GetType() switch
        {
            Type v when v == typeof(DateTime) => DateTime.Parse(toString(Value)).ToString("yyyy-MM-dd hh:mm:ss.fff"),
            Type v when v == typeof(DateTimeOffset) => ((DateTimeOffset)Value).ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss.fff"),

            _ => value?.ToString() ?? "<null>",
        };

        string toString(object? value) => value switch
        {
            null => string.Empty,
            var v => v.ToString()!,
        };
    }
}