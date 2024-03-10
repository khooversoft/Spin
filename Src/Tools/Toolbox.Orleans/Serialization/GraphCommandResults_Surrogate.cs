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
public sealed class GraphQueryResults_SurrogateConverter : IConverter<GraphQueryResults, GraphQueryResults_Surrogate>
{
    public GraphQueryResults ConvertFromSurrogate(in GraphQueryResults_Surrogate surrogate) => surrogate.Json.ToObject<GraphQueryResults>().NotNull();

    public GraphQueryResults_Surrogate ConvertToSurrogate(in GraphQueryResults value) => new GraphQueryResults_Surrogate
    {
        Json = value.ToJson(),
    };
}