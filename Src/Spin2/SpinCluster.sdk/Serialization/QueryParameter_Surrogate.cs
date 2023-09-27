using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Types;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct QueryParameter_Surrogate
{
    [Id(0)] public int Index;
    [Id(1)] public int Count;
    [Id(2)] public string? Filter;
    [Id(3)] public bool Recurse;
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
    };

    public QueryParameter_Surrogate ConvertToSurrogate(in QueryParameter value) => new QueryParameter_Surrogate
    {
        Index = value.Index,
        Count = value.Count,
        Filter = value.Filter,
        Recurse = value.Recurse,
    };
}