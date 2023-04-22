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
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public static JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public static JsonSerializerOptions PascalOptions { get; } = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    };

    public T? Deserialize<T>(string subject) => JsonSerializer.Deserialize<T>(subject, JsonSerializerOptions);
    public string Serialize<T>(T subject) => JsonSerializer.Serialize(subject, JsonSerializerOptions);
    public string SerializePascal<T>(T subject) => JsonSerializer.Serialize(subject, PascalOptions);
    public string SerializeFormat<T>(T subject) => JsonSerializer.Serialize(subject, JsonSerializerFormatOption);
    public string SerializeDefault<T>(T subject) => JsonSerializer.Serialize(subject);

    public static string ExpandNode<T>(T subject, string nodeName)
    {
        subject.NotNull();

        var jsonObject = JsonNode.Parse(subject).NotNull();
        string content = jsonObject[nodeName].NotNull().GetValue<string>();

        jsonObject[nodeName] = JsonNode.Parse(content);

        return jsonObject.ToJsonString();
    }
}