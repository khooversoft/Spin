using System.Collections.Immutable;
using Azure;
using Toolbox.Tools;

namespace Toolbox.Types;

public readonly struct DataETag : IEquatable<DataETag>
{
    public DataETag(byte[] data) => Data = ImmutableArray.Create<byte>(data.NotNull());
    public DataETag(byte[] data, ETag? etag) => (Data, ETag) = (ImmutableArray.Create<byte>(data.NotNull()), etag);

    public ImmutableArray<byte> Data { get; }
    public ETag? ETag { get; }

    public static implicit operator DataETag(byte[] data) => new(data);

    public static bool operator ==(DataETag left, DataETag right) => left.Equals(right);
    public static bool operator !=(DataETag left, DataETag right) => !(left == right);
    public override bool Equals(object? obj) => obj is DataETag tag && Equals(tag);
    public bool Equals(DataETag other) => !other.Data.IsDefault && Data.SequenceEqual(other.Data) && ETag == other.ETag;
    public override int GetHashCode() => HashCode.Combine(Data, ETag);

    public static IValidator<DataETag> Validator { get; } = new Validator<DataETag>()
        .RuleFor(x => x.Data).NotNull()
        .RuleFor(x => x.Data).Must(x => x.Length > 0, x => $"Data {x.Length} is invalid")
        .Build();

}


public static class DataETagExtensions
{
    public static Option Validate(this DataETag subject) => DataETag.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this DataETag subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
