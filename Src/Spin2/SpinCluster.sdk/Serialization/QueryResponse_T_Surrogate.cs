using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Types;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct QueryResponse_T_Surrogate<T>
{
    [Id(0)] public QueryParameter Query;
    [Id(1)] public IReadOnlyList<T> Items;
    [Id(2)] public bool EndOfSearch;
}


[RegisterConverter]
public sealed class QueryParameter_SurrogateConverter<T> : IConverter<QueryResponse<T>, QueryResponse_T_Surrogate<T>>
{
    public QueryResponse<T> ConvertFromSurrogate(in QueryResponse_T_Surrogate<T> surrogate) => new QueryResponse<T>
    {
        Query = surrogate.Query,
        Items = surrogate.Items,
        EndOfSearch = surrogate.EndOfSearch,
    };

    public QueryResponse_T_Surrogate<T> ConvertToSurrogate(in QueryResponse<T> value) => new QueryResponse_T_Surrogate<T>
    {
        Query = value.Query,
        Items = value.Items,
        EndOfSearch = value.EndOfSearch,
    };
}
