using System.Text.Json;
using System.Text.Json.Serialization;

namespace Toolbox.Tools
{
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

        public T? Deserialize<T>(string subject) => JsonSerializer.Deserialize<T>(subject, JsonSerializerOptions);
        public string Serialize<T>(T subject) => JsonSerializer.Serialize(subject, JsonSerializerOptions);
        public string SerializeFormat<T>(T subject) => JsonSerializer.Serialize(subject, JsonSerializerFormatOption);
        public string SerializeDefault<T>(T subject) => JsonSerializer.Serialize(subject);
    }
}