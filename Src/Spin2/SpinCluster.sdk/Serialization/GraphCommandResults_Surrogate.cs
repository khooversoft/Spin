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
public sealed class GraphCommandResults_SurrogateConverter : IConverter<GraphCommandResults, GraphCommandResults_Surrogate>
{
    public GraphCommandResults ConvertFromSurrogate(in GraphCommandResults_Surrogate surrogate) => surrogate.Json.ToObject<GraphCommandResults>().NotNull();

    public GraphCommandResults_Surrogate ConvertToSurrogate(in GraphCommandResults value) => new GraphCommandResults_Surrogate
    {
        Json = value.ToJson(),
    };
}