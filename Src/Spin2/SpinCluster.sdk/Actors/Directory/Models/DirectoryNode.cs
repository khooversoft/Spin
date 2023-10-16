using Toolbox.Data;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Directory;

[GenerateSerializer, Immutable]
public sealed record DirectoryNode
{
    [Id(0)] public string Key { get; init; } = null!;
    [Id(1)] public string? Tags { get; init; }
    [Id(2)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;

    public static IValidator<DirectoryNode> Validator { get; } = new Validator<DirectoryNode>()
        .RuleFor(x => x.Key).NotEmpty()
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .Build();
}


public static class DirectoryNodeExtensions
{
    public static Option Validate(this DirectoryNode subject) => DirectoryNode.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this DirectoryNode subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static DirectoryNode ConvertTo(this IGraphNode<string> subject) => new DirectoryNode
    {
        Key = subject.Key,
        Tags = subject.Tags.ToString(),
        CreatedDate = subject.CreatedDate,
    };
}