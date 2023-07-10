using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Data;

public sealed record BlobPackage
{
    public string ObjectId { get; init; } = null!;
    public string TypeName { get; init; } = null!;
    public byte[] Content { get; init; } = null!;
    public string? ETag { get; init; }
    public string? Tags { get; init; } = null!;

    public bool Equals(BlobPackage? obj)
    {
        return obj is BlobPackage otherBlob &&
               ObjectId == otherBlob.ObjectId &&
               TypeName == otherBlob.TypeName &&
               Content.SequenceEqual(otherBlob.Content) &&
               ETag == otherBlob.ETag &&
               Tags == otherBlob.Tags;
    }

    public override int GetHashCode() => HashCode.Combine(ObjectId, TypeName, Content, ETag, Tags);
    public override string ToString() => $"ObjectId={ObjectId}, TypeName={TypeName}, ETag={ETag}, Tags={Tags}";
}


public static class StorageBlobValidator
{
    public static IValidator<BlobPackage> Validator { get; } = new Validator<BlobPackage>()
        .RuleFor(x => x.ObjectId).NotEmpty().ValidObjectId()
        .RuleFor(x => x.TypeName).NotEmpty()
        .RuleFor(x => x.Content).NotNull()
        .RuleForObject(x => x).Must(x => x.IsHashVerify(), _ => "Blob's hash validation failed")
        .Build();

    public static bool IsValid(this BlobPackage subject, ScopeContextLocation location) => subject.Validate(location).IsValid;

    public static ValidatorResult Validate(this BlobPackage subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);

    public static BlobPackage WithHash(this BlobPackage blob) => blob.NotNull() with { ETag = blob.ComputeHash() };

    public static string ComputeHash(this BlobPackage blob) => new object?[]
    {
        blob.ObjectId,
        blob.TypeName,
        blob.Content,
        blob.Tags,
    }.ComputeHash()
    .Func(x => Convert.ToBase64String(x));

    public static bool IsHashVerify(this BlobPackage blob)
    {
        blob.NotNull();

        string hashBase64 = blob.ComputeHash();
        return blob.ETag == hashBase64;
    }

    public static T ToObject<T>(this BlobPackage blob)
    {
        return typeof(T) switch
        {
            Type type when type == typeof(string) => (T)(object)blob.Content.BytesToString(),
            Type type when type == typeof(byte[]) => (T)(object)blob.Content,

            _ => Json.Default.Deserialize<T>(blob.Content.BytesToString()).NotNull(),
        };
    }
}
