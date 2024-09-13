using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

[GenerateSerializer]
public struct GraphQueryResults_Surrogate
{
    [Id(0)] public string Json;
}

[RegisterConverter]
public sealed class GraphQueryResults_SurrogateConverter : IConverter<QueryBatchResult, GraphQueryResults_Surrogate>
{
    public QueryBatchResult ConvertFromSurrogate(in GraphQueryResults_Surrogate surrogate) => surrogate.Json.ToObject<QueryBatchResult>().NotNull();

    public GraphQueryResults_Surrogate ConvertToSurrogate(in QueryBatchResult value) => new GraphQueryResults_Surrogate
    {
        Json = value.ToJson(),
    };
}