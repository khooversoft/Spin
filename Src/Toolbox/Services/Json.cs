using System.Text.Json;
using System.Text.Json.Serialization;

namespace Toolbox.Services
{
    /// <summary>
    /// Provides json services using .net core JSON
    /// </summary>
    public class Json : IJson
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };

        private static readonly JsonSerializerOptions _formatOption = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };

        public static Json Default { get; } = new Json();

        public string Serialize<T>(T subject) => JsonSerializer.Serialize(subject, _options);

        public string SerializeFormat<T>(T subject) => JsonSerializer.Serialize(subject, _formatOption);

        public T Deserialize<T>(string subject) => JsonSerializer.Deserialize<T>(subject, _options);
    }
}
