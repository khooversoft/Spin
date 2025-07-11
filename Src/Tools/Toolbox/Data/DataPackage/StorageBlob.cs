using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public sealed record StorageBlob
{
    public string StorageId { get; init; } = null!;
    public byte[] Content { get; init; } = null!;
    public string? ETag { get; init; }
    public string BlobHash { get; init; } = null!;
    public string? Tags { get; init; }

    public bool Equals(StorageBlob? obj) => obj is StorageBlob document &&
        StorageId == document.StorageId &&
        Content.SequenceEqual(document.Content) &&
        ETag == document.ETag &&
        BlobHash == document.BlobHash &&
        Tags == document.Tags;

    public override int GetHashCode() => HashCode.Combine(StorageId, Content);

    public static IValidator<StorageBlob> Validator { get; } = new Validator<StorageBlob>()
        .RuleFor(x => x.StorageId).NotEmpty()
        .RuleFor(x => x.Content).NotNull()
        .RuleFor(x => x.BlobHash).NotEmpty()
        .Build();
}

public static class StorageBlobExtensions
{
    public static Option Validate(this StorageBlob subject) => StorageBlob.Validator.Validate(subject).ToOptionStatus();

    public static string CalculateHash(this StorageBlob subject)
    {
        string h1 = (subject.StorageId + subject.ETag).ToBytes().ToHexHash();
        string h2 = subject.Content.ToHexHash();
        return (h1 + h2).ToBytes().ToHexHash();
    }
}
