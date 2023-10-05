using Toolbox.Data;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct DataObjectSet_Surrogate
{
    public IReadOnlyDictionary<string, DataObject> Items;
}


[RegisterConverter]
public sealed class DataObjectSet_SurrogateConverter : IConverter<DataObjectSet, DataObjectSet_Surrogate>
{
    public DataObjectSet ConvertFromSurrogate(in DataObjectSet_Surrogate surrogate) => new DataObjectSet(
        surrogate.Items
            ?.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase) ??
            new Dictionary<string, DataObject>(StringComparer.OrdinalIgnoreCase)
    );

    public DataObjectSet_Surrogate ConvertToSurrogate(in DataObjectSet value) => new DataObjectSet_Surrogate
    {
        Items = value
            ?.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase) ??
            new Dictionary<string, DataObject>(StringComparer.OrdinalIgnoreCase)
    };
}