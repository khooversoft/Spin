using System.Reflection.Metadata;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Storage;

[GenerateSerializer, Immutable]
public sealed record StorageBlob
{
    [Id(0)] public string ObjectId { get; init; } = null!;
    [Id(1)] public string TypeName { get; init; } = null!;
    [Id(2)] public byte[] Content { get; init; } = null!;
    [Id(3)] public string? HashBase64 { get; init; }
    [Id(4)] public string? Tags { get; init; } = null!;

    public bool Equals(StorageBlob? obj)
    {
        return obj is StorageBlob document &&
               ObjectId == document.ObjectId &&
               TypeName == document.TypeName &&
               Content.SequenceEqual(document.Content) &&
               HashBase64 == document.HashBase64 &&
               Tags == document.Tags;
    }

    public override int GetHashCode() => HashCode.Combine(ObjectId, TypeName, Content, HashBase64, Tags);

    public override string ToString() => $"ObjectId={ObjectId}, TypeName={TypeName}, HashBase64={HashBase64}, Tags={Tags}";
}

public static class StorageBlobValidator
{
    public static Validator<StorageBlob> Validator { get; } = new Validator<StorageBlob>()
        .RuleFor(x => x.ObjectId).NotEmpty().Must(x => ObjectId.IsValid(x), _ => $"not a valid ObjectId, syntax={ObjectId.Syntax}")
        .RuleFor(x => x.TypeName).NotEmpty()
        .RuleFor(x => x.Content).NotNull()
        .RuleFor(x => x.HashBase64).NotNull()
        .Build();

    public static ValidatorResult Validate(this StorageBlob subject) => Validator.Validate(subject);

    public static StorageBlob Verify(this StorageBlob subject) => subject.Action(x => x.Validate().ThrowOnError());
}
