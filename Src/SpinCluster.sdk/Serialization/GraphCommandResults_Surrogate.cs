using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct GraphCommandResults_Surrogate
{
    [Id(0)] public string Json;
}

[RegisterConverter]
public sealed class GraphCommandResults_SurrogateConverter : IConverter<QueryBatchResult, GraphCommandResults_Surrogate>
{
    public QueryBatchResult ConvertFromSurrogate(in GraphCommandResults_Surrogate surrogate) => surrogate.Json.ToObject<QueryBatchResult>().NotNull();

    public GraphCommandResults_Surrogate ConvertToSurrogate(in QueryBatchResult value) => new GraphCommandResults_Surrogate
    {
        Json = value.ToJson(),
    };
}