using System.Text.Json.Serialization;
using Toolbox.Tools;

namespace Toolbox.Data;

public class GraphCoreSerialization
{
    public IReadOnlyList<Node> Nodes { get; init; } = Array.Empty<Node>();
    public IReadOnlyList<Edge> Edges { get; init; } = Array.Empty<Edge>();
}

public static class GraphCoreSerializationTool
{
    public static GraphCoreSerialization ToSerialization(this GraphCore graphCore)
    {
        graphCore.NotNull();

        return new GraphCoreSerialization
        {
            Nodes = graphCore.Nodes.ToArray(),
            Edges = graphCore.Edges.ToArray(),
        };
    }

    public static GraphCore FromSerialization(this GraphCoreSerialization serialization)
    {
        serialization.NotNull();
        var graphCore = new GraphCore(serialization);
        return graphCore;
    }
}

[JsonRegister(typeof(GraphCoreSerialization))]
[JsonSourceGenerationOptions(WriteIndented = false, DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
[JsonSerializable(typeof(GraphCoreSerialization))]
internal partial class GraphCoreSerializationContext : JsonSerializerContext
{
}
