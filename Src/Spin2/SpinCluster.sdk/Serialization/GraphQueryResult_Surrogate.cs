using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct GraphQueryResult_Surrogate
{
    [Id(0)] public string Json;
}


[RegisterConverter]
public sealed class GraphQueryResult_SurrogateConverter : IConverter<GraphQueryResult, GraphQueryResult_Surrogate>
{
    public GraphQueryResult ConvertFromSurrogate(in GraphQueryResult_Surrogate surrogate) => surrogate.Json.ToObject<GraphQueryResult>().NotNull();

    public GraphQueryResult_Surrogate ConvertToSurrogate(in GraphQueryResult value) => new GraphQueryResult_Surrogate
    {
        Json = value.ToJson(),
    };
}