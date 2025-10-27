using System.Text.Json;
using System.Text.Json.Serialization;
using Toolbox.Tools;

namespace Toolbox.Graph;

public class GraphSerialization
{
    public string? LastLogSequenceNumber { get; init; } = null;
    public IEnumerable<GraphNode> Nodes { get; init; } = Array.Empty<GraphNode>();
    public IEnumerable<GraphEdge> Edges { get; init; } = Array.Empty<GraphEdge>();
    public IEnumerable<GroupPolicy> SecurityGroups { get; init; } = Array.Empty<GroupPolicy>();
    public IEnumerable<PrincipalIdentity> PrincipalIdentities { get; init; } = Array.Empty<PrincipalIdentity>();
}

public static class GraphSerializationTool
{
    public static string ToJson(this GraphMap subject) => subject.ToSerialization().ToJson();

    public static GraphSerialization ToSerialization(this GraphMap subject) => new GraphSerialization
    {
        LastLogSequenceNumber = subject.LastLogSequenceNumber,
        Nodes = subject.Nodes,
        Edges = subject.Edges,
        SecurityGroups = subject.GrantControl.Groups,
        PrincipalIdentities = subject.GrantControl.Principals
    };

    public static string SerializeMap(this GraphMap subject)
    {
        var data = subject.ToSerialization();
        return JsonSerializer.Serialize(data, GraphJsonContext.Default.GraphSerialization);
    }

    public static GraphMap DeserializeMap(string json)
    {
        var data = JsonSerializer.Deserialize(json, GraphJsonContext.Default.GraphSerialization).NotNull();
        return data.FromSerialization();
    }

    public static GraphMap FromSerialization(this GraphSerialization subject) =>
        new GraphMap(subject, new GraphMapCounter());

    public static GraphMap FromSerialization(this GraphSerialization subject, GraphMapCounter mapCounters) =>
        new GraphMap(subject, mapCounters);
}

// System.Text.Json source generation context for fast serialization/deserialization.
//namespace Toolbox.Graph;

[JsonSourceGenerationOptions(
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never
)]
[JsonSerializable(typeof(GraphSerialization))]
[JsonSerializable(typeof(GraphNode))]
[JsonSerializable(typeof(GraphEdge))]
[JsonSerializable(typeof(GroupPolicy))]
[JsonSerializable(typeof(PrincipalIdentity))]
[JsonSerializable(typeof(GraphLink))]
internal partial class GraphJsonContext : JsonSerializerContext
{
}
