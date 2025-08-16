using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Toolbox.Tools;

/// <summary>
/// Provides json services using .net core JSON
/// </summary>
public class Json
{
    public static Json Default { get; } = new Json();

    public static JsonSerializerOptions JsonSerializerFormatOption { get; } = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            new ImmutableByteArrayConverter(),
        },
    };

    public static JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            new ImmutableByteArrayConverter(),
        },
    };

    public static JsonSerializerOptions PascalOptions { get; } = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

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

        JsonObject sourceJsonObject = JsonNode.Parse(sourceJson).NotNull().AsObject();

        if (sourceJsonObject.TryGetPropertyValue(nodeName, out var _)) sourceJsonObject.Remove(nodeName);

        JsonObject jsonObject = JsonObject.Parse(nodeJson).NotNull().AsObject();
        sourceJsonObject.Add(nodeName, jsonObject);

        return sourceJsonObject.ToJsonString(JsonSerializerOptions);
    }

    public static string WrapNode(string sourceJson, string nodeName)
    {
        sourceJson.NotEmpty();
        nodeName.NotEmpty();

        JsonObject sourceJsonObject = JsonNode.Parse(sourceJson).NotNull().AsObject();

        if (!sourceJsonObject.TryGetPropertyValue(nodeName, out var node)) throw new ArgumentException($"Cannot find nodeName={nodeName}");
        sourceJsonObject.Remove(nodeName);

        string nodeJson = node.NotNull().ToJsonString(JsonSerializerOptions).NotEmpty();
        sourceJsonObject.Add(nodeName, nodeJson);

        return sourceJsonObject.ToJsonString(JsonSerializerOptions);
    }
}
