using Toolbox.Data;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct DataObjectSerialization
{
    public string Key;
    public string TypeName;
    public IReadOnlyList<KeyValuePair<string, string>> Values;
}


[RegisterConverter]
public sealed class DataObjectSerializationConverter : IConverter<DataObject, DataObjectSerialization>
{
    public DataObject ConvertFromSurrogate(in DataObjectSerialization surrogate) => new DataObject
    {
        Key = surrogate.Key,
        TypeName = surrogate.TypeName,
        Values = surrogate.Values?.ToArray() ?? Array.Empty<KeyValuePair<string, string>>(),
    };

    public DataObjectSerialization ConvertToSurrogate(in DataObject value) => new DataObjectSerialization
    {
        Key = value.Key,
        TypeName = value.TypeName,
        Values = value.Values?.ToArray() ?? Array.Empty<KeyValuePair<string, string>>(),
    };
}
