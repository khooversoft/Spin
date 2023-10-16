using Toolbox.Data;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Directory;

[GenerateSerializer, Immutable]
public sealed record DirectoryEdge
{
    [Id(0)] public Guid Key { get; init; }
    [Id(1)] public string FromKey { get; init; } = null!;
    [Id(2)] public string ToKey { get; init; } = null!;
    [Id(3)] public string EdgeType { get; init; } = "default";
    [Id(4)] public string? Tags { get; init; }
    [Id(5)] public DateTime CreatedDate { get; init; }

    public static IValidator<DirectoryEdge> Validator { get; } = new Validator<DirectoryEdge>()
        .RuleFor(x => x.FromKey).NotEmpty()
        .RuleFor(x => x.ToKey).NotEmpty()
        .RuleFor(x => x.EdgeType).NotEmpty()
        .Build();
}


public static class DirectoryEdgeExtensions
{
    public static Option Validate(this DirectoryEdge subject) => DirectoryEdge.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this DirectoryEdge subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static DirectoryEdge ConvertTo(this IGraphEdge<string> subject) => new DirectoryEdge
    {
        Key = subject.Key,
        FromKey = subject.FromKey,
        ToKey = subject.ToKey,
        EdgeType = subject.EdgeType,
        Tags = subject.Tags.ToString(),
        CreatedDate = subject.CreatedDate,
    };
}
