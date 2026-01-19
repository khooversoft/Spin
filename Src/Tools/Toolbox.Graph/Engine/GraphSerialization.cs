using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphSerialization
{
    public string? LastLogSequenceNumber { get; init; } = null;
    public IEnumerable<GraphNode> Nodes { get; init; } = Array.Empty<GraphNode>();
    public IEnumerable<GraphEdge> Edges { get; init; } = Array.Empty<GraphEdge>();
    public IEnumerable<GroupPolicy> SecurityGroups { get; init; } = Array.Empty<GroupPolicy>();
    public IEnumerable<PrincipalIdentity> PrincipalIdentities { get; init; } = Array.Empty<PrincipalIdentity>();

    public static IValidator<GraphSerialization> Validator { get; } = new Validator<GraphSerialization>()
        .RuleFor(x => x.Nodes).NotNull()
        .RuleFor(x => x.Edges).NotNull()
        .RuleFor(x => x.SecurityGroups).NotNull()
        .RuleFor(x => x.PrincipalIdentities).NotNull()
        .Build();
}

public static class GraphSerializationTool
{
    public static Option Validate(this GraphSerialization subject) => GraphSerialization.Validator.Validate(subject).ToOptionStatus();

    public static string ToJson(this GraphMap subject) => subject.ToSerialization().ToJson();

    public static GraphSerialization ToSerialization(this GraphMap subject) => new GraphSerialization
    {
        LastLogSequenceNumber = subject.LastLogSequenceNumber,
        Nodes = subject.Nodes,
        Edges = subject.Edges,
        SecurityGroups = subject.GrantControl.Groups,
        PrincipalIdentities = subject.GrantControl.Principals
    };

    public static GraphMap FromSerialization(this GraphSerialization subject, IServiceProvider service)
    {
        subject.NotNull();
        service.NotNull();

        return ActivatorUtilities.CreateInstance<GraphMap>(service, subject);
    }

    public static void BuildSerializer()
    {
        var o = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true),
                new ImmutableByteArrayConverter(),
            },
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        };

        o.TypeInfoResolverChain.Add(GraphJsonContext.Default);
    }
}


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
