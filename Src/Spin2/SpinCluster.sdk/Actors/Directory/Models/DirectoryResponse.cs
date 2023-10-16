using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Directory;

[GenerateSerializer, Immutable]
public sealed record DirectoryResponse
{
    [Id(0)] public IReadOnlyList<DirectoryNode> Nodes { get; init; } = Array.Empty<DirectoryNode>();
    [Id(1)] public IReadOnlyList<DirectoryEdge> Edges { get; init; } = Array.Empty<DirectoryEdge>();

    public static IValidator<DirectoryResponse> Validator { get; } = new Validator<DirectoryResponse>()
        .RuleForEach(x => x.Nodes).Validate(DirectoryNode.Validator)
        .RuleForEach(x => x.Edges).Validate(DirectoryEdge.Validator)
        .Build();
}


public static class DirectoryResponseExtensions
{
    public static Option Validate(this DirectoryResponse subject) => DirectoryResponse.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this DirectoryResponse subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
