using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;

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

    public static ValidatorResult Validate(this ResourceFile subject) => Validator.Validate(subject);

    public static ResourceFile Verify(this ResourceFile subject) => subject.Action(x => x.Validate().ThrowOnError());
}
