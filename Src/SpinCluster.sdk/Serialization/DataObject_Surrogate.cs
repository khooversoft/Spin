using Toolbox.Data;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct DataObject_Surrogate
{
    public string Key;
    public string TypeName;
    public string JsonData;
    public string? Tags;
}


[RegisterConverter]
public sealed class DataObject_SurrogateConverter : IConverter<DataObject, DataObject_Surrogate>
{
    public DataObject ConvertFromSurrogate(in DataObject_Surrogate surrogate) => new DataObject
    {
        Key = surrogate.Key,
        TypeName = surrogate.TypeName,
        JsonData = surrogate.JsonData,
        Tags = surrogate.Tags,
    };

    public DataObject_Surrogate ConvertToSurrogate(in DataObject value) => new DataObject_Surrogate
    {
        Key = value.Key,
        TypeName = value.TypeName,
        JsonData = value.JsonData,
        Tags = value.Tags,
    };
}
