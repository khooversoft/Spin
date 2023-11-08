using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Actors.Directory;

[GenerateSerializer, Immutable]
public sealed record DirectoryEdgeUpdate
{
    [Id(0)] public string FromKey { get; init; } = null!;
    [Id(1)] public string ToKey { get; init; } = null!;
    [Id(2)] public string? MatchEdgeType { get; init; }
    [Id(3)] public string? UpdateEdgeType { get; init; }
    [Id(3)] public string? UpdateTags { get; init; }

    public static IValidator<DirectoryEdgeUpdate> Validator { get; } = new Validator<DirectoryEdgeUpdate>()
        .RuleFor(x => x.FromKey).NotEmpty()
        .RuleFor(x => x.ToKey).NotEmpty()
        .RuleForObject(x => x).Must(x => x.UpdateEdgeType.IsNotEmpty() || x.UpdateTags.IsNotEmpty(), _ => "Must have UpdateType or UpdateTags")
        .Build();
}


public static class DirectoryEdgeUpdateExtensions
{
    public static GraphEdgeSearch ConvertTo(this DirectoryEdgeUpdate subject) => new GraphEdgeSearch
    {
        FromKey = subject.FromKey,
        ToKey = subject.ToKey,
        EdgeType = subject.MatchEdgeType,
    };
}
