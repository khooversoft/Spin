using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Toolbox.Block.Test.Serialzation;

public class TreeDataTests
{
    [Fact]
    public void DataBlockTest()
    {
        var v = new DataArray()
            + new DataValue("key1", "value1")
            + new DataValue("key2", "value2");
    }
}

public interface IDataElement { }

public record DataValue : IDataElement
{
    public DataValue() { }

    [SetsRequiredMembers]
    public DataValue(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public required string Key { get; init; }
    public required string Value { get; init; }
}

public record DataArrayElement : IDataElement
{
    public DataArrayElement() { }

    [SetsRequiredMembers]
    public DataArrayElement(string key, IDataElement value)
    {
        Key = key;
        Value = value;
    }

    public required string Key { get; init; }
    public required IDataElement Value { get; init; }
}

public record DataArray : IDataElement
{
    public DataArray() { }

    [SetsRequiredMembers]
    public DataArray(IEnumerable<IDataElement> elements)
    {
        Values = elements?.ToArray() ?? Array.Empty<IDataElement>();
    }

    public IReadOnlyList<IDataElement> Values { get; init; } = Array.Empty<IDataElement>();

    public DataArray Add(IDataElement value) => value switch
    {
        null => this,
        var v => this with
        {
            Values = Values.Append(v).ToArray(),
        },
    };

    public DataArray Add(IEnumerable<IDataElement> value) => value switch
    {
        null => this,
        var v => this with
        {
            Values = v.Concat(value).ToArray(),
        },
    };

    public static DataArray operator +(DataArray subject, IDataElement value) => subject.Add(value);
    public static DataArray operator +(DataArray subject, IEnumerable<IDataElement> value) => subject.Add(value);
}
