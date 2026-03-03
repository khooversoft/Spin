using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphSerialization
{
    public string? LastLogSequenceNumber { get; init; } = null;
    public IEnumerable<GraphNode> Nodes { get; init; } = Array.Empty<GraphNode>();
    public IEnumerable<GraphEdge> Edges { get; init; } = Array.Empty<GraphEdge>();
    public GraphCoreSerialization GrantControl { get; init; } = null!;

    public static IValidator<GraphSerialization> Validator { get; } = new Validator<GraphSerialization>()
        .RuleFor(x => x.Nodes).NotNull()
        .RuleFor(x => x.Edges).NotNull()
        .RuleFor(x => x.GrantControl).NotNull()
        .Build();
}

public static class GraphSerializationTool
{
    public static Option Validate(this GraphSerialization subject) => GraphSerialization.Validator.Validate(subject).ToOptionStatus();

    public static GraphSerialization ToSerialization(this GraphMap subject) => new GraphSerialization
    {
        LastLogSequenceNumber = subject.LastLogSequenceNumber,
        Nodes = subject.Nodes,
        Edges = subject.Edges,
        GrantControl = subject.GrantControl.GetGraph().ToSerialization(),
    };

    public static GraphMap FromSerialization(this GraphSerialization subject, IServiceProvider service)
    {
        subject.NotNull();
        service.NotNull();

        return ActivatorUtilities.CreateInstance<GraphMap>(service, subject);
    }
}


[JsonSourceGenerationOptions(WriteIndented = false, DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
[JsonRegister(typeof(GraphSerialization))]
[JsonRegister(typeof(GraphNode))]
[JsonRegister(typeof(GraphEdge))]
[JsonRegister(typeof(PrincipalGrant))]
[JsonRegister(typeof(PrincipalIdentity))]
[JsonRegister(typeof(GraphLink))]
[JsonSerializable(typeof(GraphSerialization))]
[JsonSerializable(typeof(GraphNode))]
[JsonSerializable(typeof(GraphEdge))]
[JsonSerializable(typeof(PrincipalGrant))]
[JsonSerializable(typeof(PrincipalIdentity))]
[JsonSerializable(typeof(GraphLink))]
internal partial class GraphJsonContext : JsonSerializerContext
{
}
