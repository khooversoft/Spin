using System;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace MessageNet.sdk.Protocol
{
    /// <summary>
    /// Endpoint ID is... {namespace}/{node}[/{endpoint}]
    ///
    /// </summary>
    public record EndpointId
    {
        private string _id = string.Empty!;

        public EndpointId()
        {
        }

        public EndpointId(string id) => Id = ToId(id);

        public EndpointId(string nameSpace, string node, string? endpoint)
            : this(ToId(nameSpace, node, endpoint))
        {
        }

        [JsonIgnore]
        public string? Endpoint => Id.Split('/').Skip(2).FirstOrDefault() ?? string.Empty;

        public string Id { get => _id; init => _id = ToId(value); }

        [JsonIgnore]
        public string Namespace => Id.Split('/')[0];

        [JsonIgnore]
        public string Node => Id.Split('/').Skip(1).FirstOrDefault() ?? string.Empty;

        public static explicit operator EndpointId(string id) => new EndpointId(id);

        public static explicit operator string(EndpointId endpointId) => endpointId.ToString();

        public static EndpointId FromBase64(string base64) => new EndpointId(Encoding.UTF8.GetString(Convert.FromBase64String(base64)));

        public string ToBase64() => Convert.ToBase64String(Encoding.UTF8.GetBytes(Id));

        public override string ToString() => Id;

        private static string ToId(string nameSpace, string node, string? endPoint) => new[]
        {
            nameSpace,
            node,
        }
        .Where(x => !x.IsEmpty())
        .Func(x => string.Join('/', x))
        .ToLower();

        private static string ToId(string id)
        {
            id.VerifyNotEmpty(id);

            id = id.ToLower();
            VerifyId(id);

            return id.Split('/')
                .Take(3)
                .Func(x => string.Join('/', x));
        }

        private static void VerifyId(string endpointId)
        {
            endpointId.VerifyAssert(x => x.All(y => char.IsLetterOrDigit(y) || y == '.' || y == '/' || y == '-'), "Valid Id must be letter, number, '.', '/', or '-'");

            endpointId.Split('/')
                .VerifyAssert(x => x.Length switch { 2 or 3 => true, _ => false }, "id format error: {namespace}/{node}[/{endpoint}]")
                .ForEach(x =>
                {
                    x.VerifyNotEmpty("path vector is empty");
                    x.VerifyAssert(x => char.IsLetter(x[0]), $"{x} path vector start with letter");
                    x.VerifyAssert(x => char.IsLetterOrDigit(x[^1]), $"{x[^1]} path vector end with letter or number");
                });
        }
    }
}