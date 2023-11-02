using Toolbox.Data;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Actors.Directory;

[GenerateSerializer, Immutable]
public sealed record DirectoryNodeUpdate
{
    [Id(0)] public string? Key { get; init; } = null!;
    [Id(1)] public string? MatchTags { get; init; } = null!;
    [Id(1)] public string UpdateTags { get; init; } = null!;

    public static IValidator<DirectoryNodeUpdate> Validator { get; } = new Validator<DirectoryNodeUpdate>()
        .RuleFor(x => x.UpdateTags).NotEmpty()
        .Build();
}


public static class DirectoryNodeUpdateExtensions
{
    public static GraphNodeQuery<string> ConvertTo(this DirectoryNodeUpdate subject) => new GraphNodeQuery<string>
    {
        Key = subject.Key,
        Tags = subject.MatchTags,
    };
}
