using System.Collections;

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
