using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace MessageNet.sdk.Protocol
{
    /// <summary>
    /// Message URI is version of URI, format= [protocol://]{service}/{endpoint}[/{path}...]
    /// 
    /// Note: Query strings are not supported
    /// </summary>
    public record MessageUrl
    {
        const string _protocolDelimiter = "://";
        const string _syntax = "syntax: [protocol://]{service}[/{path}...]";
        private string _protocol = string.Empty;
        private string _service = string.Empty;
        private string? _endpoint;

        public MessageUrl(string url)
        {
            (string protocol, string service, string? endpoint) = Parse(url);

            Protocol = protocol;
            Service = service;
            Endpoint = endpoint;
        }

        [JsonConstructor]
        public MessageUrl(string protocol, string service, string? endpoint = null)
        {
            Protocol = protocol;
            Service = service;
            Endpoint = endpoint;
        }


        public string Protocol { get => _protocol; init => _protocol = VerifyProtocol(value); }

        public string Service { get => _service; init => _service = VerifyService(value); }

        public string? Endpoint { get => _endpoint; init => _endpoint = VerifyPath(value); }

        public override string ToString() => $"{Protocol}://{Service}{(Endpoint == null ? string.Empty : "/" + Endpoint)}";

        public static MessageUrl FromBase64(string base64) => new MessageUrl(Encoding.UTF8.GetString(Convert.FromBase64String(base64)));

        public string ToBase64() => Convert.ToBase64String(Encoding.UTF8.GetBytes(ToString()));


        public static (string protoocol, string service, string? endpoint) Parse(string uri)
        {
            (string protocol, string path) = InternalParse(uri);

            string[] parts = path.Split("/");
            string service = parts[0];

            string? endpoint = parts
                .Skip(1)
                .Func(x => string.Join("/", x))
                .ToNullIfEmpty();

            return (protocol, service, endpoint);
        }

        public static void Verify(string url) => InternalParse(url);

        public static explicit operator MessageUrl(string url) => new MessageUrl(url);

        public static explicit operator string(MessageUrl url) => url.ToString();

        public static MessageUrl operator +(MessageUrl subject, string appendPath)
        {
            subject.VerifyNotNull(nameof(subject));

            if (appendPath.IsEmpty()) return subject;

            string[] endpointVector = (subject.Endpoint ?? String.Empty).Split('/', StringSplitOptions.RemoveEmptyEntries);
            string[] appendPathVector = appendPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            string? endpoint = string.Join('/', endpointVector.Concat(appendPathVector)).ToNullIfEmpty();

            return new MessageUrl(subject.Protocol, subject.Service, endpoint);
        }

        private static (string protocol, string path) InternalParse(string url)
        {
            url.VerifyNotEmpty(url);

            bool hasDelimiter = url.IndexOf(_protocolDelimiter) >= 0;

            string[] parts = url
                .Split(_protocolDelimiter)
                .VerifyAssert(x => x.Length switch { 1 => !hasDelimiter, 2 => hasDelimiter, _ => false }, $"Invalid, missing server: {url}, {_syntax}")
                .VerifyAssert(x => x.Length switch { 1 or 2 => true, _ => false }, $"Invalid format {url}, {_syntax}");

            (string protocol, string path) = parts.Length == 1 ? (MessageProtocol.message.ToString(), parts[0]) : (parts[0], parts[1]);

            return (protocol, path);
        }

        public static string VerifyProtocol(string? value) => value
            .Func(x => VerifyAllowedCharacters(value.ToNullIfEmpty() ?? MessageProtocol.message.ToString(), '.', '-'));

        public static string? VerifyPath(string? value) => value
            .Func(x => x switch
            {
                string => VerifyAllowedCharacters(x, '.', '-', '/'),
                _ => x
            });

        public static string VerifyService(string value) => value
            .VerifyNotEmpty(_syntax)
            .Action(x => VerifyAllowedCharacters(x, '.', '-'));

        public static string VerifyAllowedCharacters(string data, params char[] allowed) => data
            .VerifyNotEmpty($"Error, {_syntax}")
            .VerifyAssert(
                x => x.All(y => char.IsLetterOrDigit(y) || allowed.Contains(y)),
                x => $"Invalid id, must be  letter, number, {string.Join(", ", allowed)}, {_syntax}"
                );
    }
}
