using Toolbox.Extensions;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Storage;

[GenerateSerializer, Immutable]
public sealed record StorageBlob
{
    [Id(0)] public string StorageId { get; init; } = null!;
    [Id(1)] public byte[] Content { get; init; } = null!;
    [Id(2)] public string? ETag { get; init; }
    [Id(3)] public string BlobHash { get; init; } = null!;

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
        .RuleFor(x => x.BlobHash).NotEmpty()
        .Build();
}

public static class StorageBlobExtensions
{
    public static Option Validate(this StorageBlob subject) => StorageBlob.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this StorageBlob subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static string CalculateHash(this StorageBlob subject)
    {
        string h1 = (subject.StorageId + subject.ETag).ToBytes().ToHexHash();
        string h2 = subject.Content.ToHexHash();
        return (h1 + h2).ToBytes().ToHexHash();
    }
}
