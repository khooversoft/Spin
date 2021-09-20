using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Model
{
    /// <summary>
    /// Universal resource id, fmt: {resourceType}://{namespace}/{id}[/{data}...]
    /// Required: type, namespace, id
    /// </summary>
    public record ResourceId
    {
        private const string _syntax = "{resourceType}://{namespace}/{id}[/{data}...]";
        private const string _errorMsg = "Valid Id must be letter, number, '.', or '-'";

        public ResourceId(string uri)
        {
            (string resourceType, string channel, string path) = Parse(uri);
            Type = resourceType;
            Channel = channel;
            Path = path;
        }

        public ResourceId(string resourceType, string channel, string? path = null)
        {
            Type = resourceType.VerifyNotEmpty(nameof(resourceType));
            Channel = channel.VerifyNotEmpty(nameof(channel));
            Path = path.ToNullIfEmpty();
        }

        public string Type { get; }

        public string Channel { get; }

        public string? Path { get; }

        public override string ToString() => $"{Type}://{Channel}{(Path == null ? string.Empty : "/" + Path)}";

        public string ToPath() => $"{Channel}{(Path ?? string.Empty)}";


        static private (string resourceType, string channel, string path) Parse(string uri)
        {
            uri.VerifyNotEmpty(nameof(uri));

            string type = uri.Split("://")
                .FirstOrDefault()
                .VerifyNotEmpty($"Resource type not found in {uri}, format: ({_syntax})")
                .VerifyAssert(x => VerifySyntax(x), _errorMsg);

            string channel = uri
                .Split("://")
                .Skip(1)
                .FirstOrDefault()
                .VerifyNotEmpty($"Channel not found in {uri}, format: ({_syntax})")
                .Split('/')
                .First()
                .VerifyAssert(x => VerifySyntax(x), _errorMsg);

            string? path = uri
                .Split("://")
                .Skip(1)
                .First()
                .Split('/')
                .Skip(1)
                .Func(x => string.Join("/", x));

            return (type, channel, path);

            static bool VerifySyntax(string value) => value.All(y => char.IsLetterOrDigit(y) || y == '.' || y == '-');
        }
    }
}
