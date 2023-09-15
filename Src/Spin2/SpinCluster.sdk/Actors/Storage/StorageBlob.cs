using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Storage;

[GenerateSerializer, Immutable]
public sealed record StorageBlob
{
    [Id(0)] public string StorageId { get; init; } = null!;
    [Id(1)] public byte[] Content { get; init; } = null!;
    [Id(2)] public string? ETag { get; init; }

    public bool Equals(StorageBlob? obj)
    {
        return obj is StorageBlob document &&
               StorageId == document.StorageId &&
               Content.SequenceEqual(document.Content);
    }

    public override int GetHashCode() => HashCode.Combine(StorageId, Content);

    public static IValidator<StorageBlob> Validator { get; } = new Validator<StorageBlob>()
        .RuleFor(x => x.StorageId).ValidResourceId(ResourceType.DomainOwned)
        .RuleFor(x => x.Content).NotNull()
        .Build();
}

public static class StorageBlobExtensions
{
    public static Option Validate(this StorageBlob subject) => StorageBlob.Validator.Validate(subject).ToOptionStatus();
}
