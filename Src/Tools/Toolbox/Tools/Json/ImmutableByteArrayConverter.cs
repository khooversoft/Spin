using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Toolbox.Tools;

public class ImmutableByteArrayConverter : JsonConverter<ImmutableArray<byte>>
{
    public override ImmutableArray<byte> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Expect a Base64 string and convert to ImmutableArray<byte> without an extra copy.
        if (reader.TokenType != JsonTokenType.String) throw new JsonException();

        byte[] bytes = reader.GetBytesFromBase64();
        return ImmutableCollectionsMarshal.AsImmutableArray(bytes);
    }

    public override void Write(Utf8JsonWriter writer, ImmutableArray<byte> value, JsonSerializerOptions options)
    {
        // Normalize default to empty and write as Base64 string.
        ReadOnlySpan<byte> span = value.IsDefault ? ReadOnlySpan<byte>.Empty : value.AsSpan();
        writer.WriteBase64StringValue(span);
    }
}
