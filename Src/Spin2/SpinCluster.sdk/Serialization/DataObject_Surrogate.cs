using Toolbox.Data;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct DataObject_Surrogate
{
    public string Key;
    public string TypeName;
    public IDictionary<string, string> Values;
}


[RegisterConverter]
public sealed class DataObject_SurrogateConverter : IConverter<DataObject, DataObject_Surrogate>
{
    public DataObject ConvertFromSurrogate(in DataObject_Surrogate surrogate) => new DataObject
    {
        Key = surrogate.Key,
        TypeName = surrogate.TypeName,
        Values = surrogate.Values?.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, string>(),
    };

    public DataObject_Surrogate ConvertToSurrogate(in DataObject value) => new DataObject_Surrogate
    {
        Key = value.Key,
        TypeName = value.TypeName,
        Values = value.Values?.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, string>(),
    };
}
