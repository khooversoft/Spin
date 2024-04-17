using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

public readonly struct DataETag<T>
{
    [JsonConstructor]
    public DataETag(T value, string? eTag = null) => (Value, ETag) = (value.NotNull(), eTag);

    public T Value { get; }
    public string? ETag { get; }
}

public readonly struct DataETag : IEquatable<DataETag>
{
    public DataETag(byte[] data) => Data = ImmutableArray.Create<byte>(data.NotNull());
    public DataETag(byte[] data, string? etag) => (Data, ETag) = (ImmutableArray.Create<byte>(data.NotNull()), etag);

    [JsonConstructor]
    public DataETag(ImmutableArray<byte> data, string? eTag) => (Data, ETag) = (data, eTag);

    public ImmutableArray<byte> Data { get; }
    public string? ETag { get; }

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

    public static DataETag ToDataETag<T>(this T value) where T : class
    {
        string json = value.ToJson();
        return new DataETag(json.ToBytes());
    }

    public static string ToHash(this DataETag data) => data.Data.ToHexHash();

    public static DataETag WithHash(this DataETag data) => new DataETag(data.Data, data.ToHash());

    public static T ToObject<T>(this DataETag data) => data.Data.AsSpan().ToObject<T>().NotNull();
}
