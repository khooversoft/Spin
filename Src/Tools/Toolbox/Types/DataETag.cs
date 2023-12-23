using Azure;
using Toolbox.Tools;

namespace Toolbox.Types;

public struct DataETag
{
    public DataETag(byte[] data) => Data = data.NotNull();
    public DataETag(byte[] data, ETag? etag) => (Data, ETag) = (data, etag);

    public byte[] Data { get; }
    public ETag? ETag { get; }

    public static implicit operator DataETag(byte[] data) => new(data);

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
