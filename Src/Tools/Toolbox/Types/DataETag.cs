using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

public readonly struct DataETag<T>
{
    public DataETag(DataETag data) => Data = data;
    public DataETag Data { get; }

    public static DataETag<T> Create(DataETag data)
    {
        if (data.Data.IsDefaultOrEmpty) throw new ArgumentException("Data cannot be empty", nameof(data));
        return new DataETag<T>(data);
    }
    public static DataETag<T> Create<TValue>(TValue value)
    {
        value.NotNull();
        DataETag data = value.ToDataETag();
        return new DataETag<T>(data);
    }
}


public readonly struct DataETag : IEquatable<DataETag>
{
    public DataETag(byte[] data) => Data = ImmutableArray.Create<byte>(data.NotNull());
    public DataETag(byte[] data, string? etag) => (Data, ETag) = (ImmutableArray.Create<byte>(data.NotNull()), etag);

    [JsonConstructor]
    public DataETag(ImmutableArray<byte> data, string? eTag) => (Data, ETag) = (data, eTag);

    public ImmutableArray<byte> Data { get; }
    public string? ETag { get; }
    public DataETag Append(DataETag append) => Data.Concat(append.Data).ToDataETag();

    public override bool Equals(object? obj) => obj is DataETag tag && Equals(tag);
    public bool Equals(DataETag other) => !other.Data.IsDefault && Data.SequenceEqual(other.Data);
    public override int GetHashCode() => HashCode.Combine(Data, ETag);


    public static implicit operator DataETag(byte[] data) => new(data);
    public static bool operator ==(DataETag left, DataETag right) => left.Equals(right);
    public static bool operator !=(DataETag left, DataETag right) => !(left == right);
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

    public static DataETag ToDataETag(this DataETag? subject) => subject switch
    {
        null => throw new ArgumentException("DataETag cannot be null"),
        { } data => data,
    };

    public static DataETag ToDataETag<T>(this T value, string? currentETag = null)
    {
        value.NotNull();

        if (value is DataETag dataTag) return dataTag;
        if (value is DataETag<T> dataTagType) return dataTagType.Data;

        var bytes = value switch
        {
            null => throw new ArgumentNullException("value"),
            IEnumerable<DataETag> => throw new ArgumentException("No array are allowed"),
            IEnumerable<byte> v => v.ToArray(),
            string v => v.ToBytes(),
            Memory<byte> v => v.ToArray(),
            var v => v.ToJson().ToBytes(),
        };

        return new DataETag(bytes, currentETag);
    }

    public static T ToObject<T>(this DataETag<T> subject) => subject.NotNull().Data.ToObject<T>();
    public static DataETag StripETag(this DataETag data) => new DataETag([.. data.Data]);
    public static string ToHash(this DataETag data) => data.Data.ToHexHash();
    public static DataETag WithHash(this DataETag data) => new DataETag(data.Data, data.ToHash());
    public static DataETag WithETag(this DataETag data, string eTag) => new DataETag(data.Data, eTag.NotEmpty());

    public static T ToObject<T>(this DataETag data) => (typeof(T) == typeof(DataETag)) switch
    {
        true => (T)(object)data,
        false => data.Data.AsSpan().ToObject<T>().NotNull("Serialization failed"),
    };

    public static string ToJsonFromData(this DataETag subject)
    {
        if (subject.Data.Length == 0) return string.Empty;

        string jsonString = subject.Data.BytesToString();
        using JsonDocument jsonDocument = JsonDocument.Parse(jsonString);
        var result = JsonSerializer.Serialize(jsonDocument.RootElement, Json.JsonSerializerFormatOption);

        return result;
    }

}
