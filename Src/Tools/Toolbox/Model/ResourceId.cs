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
        private const string _syntax = "{resourceType}:[{channel}]//{path}[{data}...]";
        private const string _errorMsg = "Valid Id must be letter, number, '.', or '-'";

        public ResourceId(string uri)
        {
            (string resourceType, string? channel, string? path) = Parse(uri);

            Type = resourceType;
            Channel = channel;
            Path = path;
        }

        public ResourceId(string resourceType, string path)
        {
            Type = resourceType.NotEmpty();
            Channel = null;
            Path = path.NotEmpty();
        }

        public ResourceId(string resourceType, string channel, string path)
        {
            Type = resourceType.NotEmpty();
            Channel = channel.NotEmpty();
            Path = path.NotEmpty();
        }

        public string Type { get; }

        public string? Channel { get; }

        public string? Path { get; }

        public override string ToString() => $"{Type}:{Channel ?? string.Empty}//{Path}";

        static private (string resourceType, string? channel, string? path) Parse(string uri)
        {
            uri.NotEmpty();

            string type = uri
                .Split("//")
                .First()
                .Split(':')
                .First()
                .NotEmpty(name: $"Resource type not found in {uri}, format: ({_syntax})")
                .Assert(x => VerifySyntax(x), _errorMsg);

            string? channel = uri
                .Split("//")
                .First()
                .Split(':')
                .Skip(1)
                .FirstOrDefault()
                ?.Assert(x => VerifySyntax(x), _errorMsg);

            string? path = uri
                .Split("//")
                .Skip(1)
                .FirstOrDefault();

            return (type, channel, path);

            static bool VerifySyntax(string value) => value.All(y => char.IsLetterOrDigit(y) || y == '.' || y == '-');
        }
    }
}
