using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

public sealed record DataETag : IEquatable<DataETag>
{
    public DataETag(byte[] data) => Data = ImmutableArray.Create<byte>(data.NotNull());
    public DataETag(byte[] data, string? etag) => (Data, ETag) = (ImmutableArray.Create<byte>(data.NotNull()), etag);

    [JsonConstructor]
    public DataETag(ImmutableArray<byte> data, string? eTag) => (Data, ETag) = (data, eTag);

    public ImmutableArray<byte> Data { get; init; } = ImmutableArray<byte>.Empty;
    public string? ETag { get; init; }

    public DataETag Append(DataETag append) => Data.Concat(append.Data).ToDataETag();

    public bool Equals(DataETag? other) => other is not null && !other.Data.IsDefault && Data.SequenceEqual(other.Data);
    public override int GetHashCode() => HashCode.Combine(Data, ETag);

    public static implicit operator DataETag(byte[] data) => new(data);
    public static DataETag operator +(DataETag left, DataETag right) => left.Append(right);

    public static IValidator<DataETag> Validator { get; } = new Validator<DataETag>()
        .RuleFor(x => x.Data).NotNull()
        .RuleFor(x => x.Data).Must(x => x.Length > 0, x => $"Data {x.Length} is invalid")
        .Build();
}

public static class DataETagExtensions
{
    public static Option Validate(this DataETag subject) => DataETag.Validator.Validate(subject).ToOptionStatus();

    public static string DataToString(this DataETag subject) => subject.Data.BytesToString();

    public static DataETag ToDataETag<T>(this T value, string? currentETag = null)
    {
        value.NotNull();
        if (value is DataETag dataTag) return dataTag;

        var bytes = value.ConvertToBytes();
        return new DataETag(bytes, currentETag);
    }

    public static DataETag ToDataETagWithHash<T>(this T value)
    {
        value.NotNull();
        if (value is DataETag dataTag) return dataTag;

        var bytes = value.ConvertToBytes();
        return new DataETag(bytes, bytes.ToHexHash());
    }

    private static byte[] ConvertToBytes<T>(this T value)
    {
        value.NotNull();

        return value switch
        {
            null => throw new ArgumentNullException("value"),
            IEnumerable<DataETag> => throw new ArgumentException("No array are allowed"),
            IEnumerable<byte> v => v.ToArray(),
            string v => v.ToBytes(),
            Memory<byte> v => v.ToArray(),
            var v => v.ToJson().ToBytes(),
        };
    }

    public static DataETag StripETag(this DataETag data) => new DataETag([.. data.Data]);
    public static string ToHash(this DataETag data) => data.Data.ToHexHash();
    public static DataETag WithHash(this DataETag data) => new DataETag(data.Data, data.ToHash());

    public static DataETag WithETag(this DataETag data, string eTag) => new DataETag(data.Data, eTag.NotEmpty());
}

[JsonSourceGenerationOptions(
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    Converters = new[] { typeof(ImmutableByteArrayConverter) })
    ]
[JsonSerializable(typeof(DataETag))]
[JsonRegister(typeof(DataETag))]
internal partial class DataETagJsonContext : JsonSerializerContext
{
}
