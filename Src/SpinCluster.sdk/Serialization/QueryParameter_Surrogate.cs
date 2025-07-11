using Toolbox.Types;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct QueryParameter_Surrogate
{
    [Id(0)] public int Index;
    [Id(1)] public int Count;
    [Id(2)] public string Filter;
    [Id(3)] public bool Recurse;
    [Id(4)] public bool IncludeFile;
    [Id(5)] public bool IncludeFolder;
    [Id(6)] public string BasePath;
}


[RegisterConverter]
public sealed class QueryParameter_SurrogateConverter : IConverter<QueryParameter, QueryParameter_Surrogate>
{
    public QueryParameter ConvertFromSurrogate(in QueryParameter_Surrogate surrogate) => new QueryParameter
    {
        Index = surrogate.Index,
        Count = surrogate.Count,
        Filter = surrogate.Filter,
        Recurse = surrogate.Recurse,
        IncludeFile = surrogate.IncludeFile,
        IncludeFolder = surrogate.IncludeFolder,
        BasePath = surrogate.BasePath,
    };

    public QueryParameter_Surrogate ConvertToSurrogate(in QueryParameter value) => new QueryParameter_Surrogate
    {
        Index = value.Index,
        Count = value.Count,
        Filter = value.Filter,
        Recurse = value.Recurse,
        IncludeFile = value.IncludeFile,
        IncludeFolder = value.IncludeFolder,
        BasePath = value.BasePath,
    };
}