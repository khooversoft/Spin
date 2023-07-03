using SpinCluster.sdk.Actors.Lease;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Resource;

[GenerateSerializer, Immutable]
public record ResourceFile
{
    [Id(0)] public string ObjectId { get; init; } = null!;
    [Id(1)] public byte[] Content { get; init; } = null!;
    [Id(2)] public string? ETag { get; init; }
}


public static class ResourceFileValidator
{
    public static Validator<ResourceFile> Validator { get; } = new Validator<ResourceFile>()
        .RuleFor(x => x.ObjectId).NotEmpty()
        .RuleFor(x => x.Content).NotNull()
        .Build();

    public static ValidatorResult Validate(this ResourceFile subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);
}
