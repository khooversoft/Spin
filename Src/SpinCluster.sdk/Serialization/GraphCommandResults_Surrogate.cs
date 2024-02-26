using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct GraphCommandResults_Surrogate
{
    [Id(0)] public string Json;
}

[RegisterConverter]
public sealed class GraphCommandResults_SurrogateConverter : IConverter<GraphQueryResults, GraphCommandResults_Surrogate>
{
    public GraphQueryResults ConvertFromSurrogate(in GraphCommandResults_Surrogate surrogate) => surrogate.Json.ToObject<GraphQueryResults>().NotNull();

    public GraphCommandResults_Surrogate ConvertToSurrogate(in GraphQueryResults value) => new GraphCommandResults_Surrogate
    {
        Json = value.ToJson(),
    };
}