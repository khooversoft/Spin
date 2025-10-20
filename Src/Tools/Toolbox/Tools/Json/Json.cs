using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Toolbox.Tools;

/// <summary>
/// Provides JSON services using System.Text.Json
/// </summary>
public class Json
{
    public static Json Default { get; } = new Json();

    // Use consistent document options for JsonNode.Parse to match serializer tolerance
    private static readonly JsonDocumentOptions s_docOptions = new JsonDocumentOptions
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    // Default resolver required before calling MakeReadOnly() on JsonSerializerOptions in .NET 8+
    private static readonly IJsonTypeInfoResolver s_resolver = new DefaultJsonTypeInfoResolver();

    public static JsonSerializerOptions JsonSerializerFormatOption { get; } = CreateIndentedOptions();
    public static JsonSerializerOptions JsonSerializerOptions { get; } = CreateDefaultOptions();
    public static JsonSerializerOptions PascalOptions { get; } = CreatePascalOptions();

    private static JsonSerializerOptions CreateDefaultOptions()
    {
        var o = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true),
                new ImmutableByteArrayConverter(),
            },
            TypeInfoResolver = s_resolver,
        };
        o.MakeReadOnly();
        return o;
    }

    private static JsonSerializerOptions CreateIndentedOptions()
    {
        var o = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true),
                new ImmutableByteArrayConverter(),
            },
            TypeInfoResolver = s_resolver,
        };
        o.MakeReadOnly();
        return o;
    }

    private static JsonSerializerOptions CreatePascalOptions()
    {
        var o = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true),
                new ImmutableByteArrayConverter(),
            },
            TypeInfoResolver = s_resolver,
        };
        o.MakeReadOnly();
        return o;
    }

    public T? Deserialize<T>(string subject) => JsonSerializer.Deserialize<T>(subject, JsonSerializerOptions);
    public string Serialize<T>(T subject) => JsonSerializer.Serialize(subject, JsonSerializerOptions);
    public string SerializePascal<T>(T subject) => JsonSerializer.Serialize(subject, PascalOptions);
    public string SerializeFormat<T>(T subject) => JsonSerializer.Serialize(subject, JsonSerializerFormatOption);
    public string SerializeDefault<T>(T subject) => JsonSerializer.Serialize(subject);

    public static string ExpandNode(string sourceJson, string nodeName, string nodeJson)
    {
        sourceJson.NotEmpty();
        nodeName.NotEmpty();
        nodeJson.NotEmpty();

        JsonObject sourceJsonObject = JsonNode.Parse(sourceJson, default, s_docOptions).NotNull().AsObject();

        // Remove if exists (Remove returns false if not present, no need to check)
        sourceJsonObject.Remove(nodeName);

        JsonObject jsonObject = JsonNode.Parse(nodeJson, default, s_docOptions).NotNull().AsObject();
        sourceJsonObject.Add(nodeName, jsonObject);

        return sourceJsonObject.ToJsonString(JsonSerializerOptions);
    }

    public static string WrapNode(string sourceJson, string nodeName)
    {
        sourceJson.NotEmpty();
        nodeName.NotEmpty();

        JsonObject sourceJsonObject = JsonNode.Parse(sourceJson, default, s_docOptions).NotNull().AsObject();

        if (!sourceJsonObject.TryGetPropertyValue(nodeName, out var node))
            throw new ArgumentException($"Cannot find nodeName={nodeName}");

        sourceJsonObject.Remove(nodeName);

        string nodeJson = node.NotNull().ToJsonString(JsonSerializerOptions).NotEmpty();
        sourceJsonObject.Add(nodeName, nodeJson);

        return sourceJsonObject.ToJsonString(JsonSerializerOptions);
    }
}
