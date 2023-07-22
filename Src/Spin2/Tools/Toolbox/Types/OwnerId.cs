using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tokenizer.Token;
using Toolbox.Tools;

namespace Toolbox.Types;

public record OwnerId
{
    private ParseDetails _details;

    public OwnerId(string ownerId) => _details = Parse(ownerId).ThrowOnError().Return();
    public OwnerId(ParseDetails parseDetails) => _details = parseDetails.NotNull();

    public string Id => _details.OwnerId;
    [JsonIgnore] public string Name => _details.Name;
    [JsonIgnore] public string Domain => _details.Domain;
    public override string ToString() => _details.OwnerId;

    public static bool IsValid(string subject) => Parse(subject).StatusCode.IsOk();

    public static Option<ParseDetails> Parse(string ownerId)
    {
        const string syntaxErrorText = "Syntax error: {name}@{domain}";

        if (ownerId.IsEmpty()) return new Option<ParseDetails>(StatusCode.BadRequest, "Null or empty");
        if (ownerId.IndexOf("..") >= 0) return new Option<ParseDetails>(StatusCode.BadRequest, syntaxErrorText);

        string[] parts = ownerId.Split('@');
        if (parts.Length != 2) return new Option<ParseDetails>(StatusCode.BadRequest, syntaxErrorText);
        if (parts[0].IsEmpty() || parts[1].IsEmpty()) return new Option<ParseDetails>(StatusCode.BadRequest, syntaxErrorText);

        // Prefix
        if (!parts[0].All(x => IsPrefixCharacterValid(x))) return new Option<ParseDetails>(StatusCode.BadRequest, "Invalid character is name");

        if (InvalidStartOrEnd(parts[0][0]) || InvalidStartOrEnd(parts[0][^1])) return new Option<ParseDetails>(StatusCode.BadRequest, syntaxErrorText);

        // Domain
        if (InvalidStartOrEnd(parts[1][0]) || InvalidStartOrEnd(parts[1][^1])) return new Option<ParseDetails>(StatusCode.BadRequest, syntaxErrorText);

        if (!parts[1].All(x => IsDomainCharacterValid(x))) return new Option<ParseDetails>(StatusCode.BadRequest, "Invalid characters in domain");

        string[] domainParts = parts[1].Split('.');
        if (domainParts.Length != 2) return new Option<ParseDetails>(StatusCode.BadRequest, "No domain root");

        return new ParseDetails
        {
            OwnerId = ownerId,
            Name = parts[0],
            Domain = parts[1],
        };
    }

    public static Option<OwnerId> CreateIfValid(string ownerId, ScopeContext context)
    {
        Option<ParseDetails> parseDetails = Parse(ownerId).LogResult(context.Location());
        if (parseDetails.IsError()) return parseDetails.ToOption<OwnerId>();

        return new OwnerId(parseDetails.Return());
    }

    private static bool IsPrefixCharacterValid(char ch) => char.IsLetterOrDigit(ch) || ch == '.' || ch == '-' || ch == '@' || ch == '_';
    private static bool IsDomainCharacterValid(char ch) => char.IsLetterOrDigit(ch) || ch == '-' || ch == '.';
    private static bool InvalidStartOrEnd(char ch) => ch == '.' || ch == '-' || ch == '_';

    public static bool operator ==(OwnerId left, string right) => left.Id.Equals(right);
    public static bool operator !=(OwnerId left, string right) => !(left == right);

    public readonly record struct ParseDetails
    {
        public string OwnerId { get; init; }
        public string Name { get; init; }
        public string Domain { get; init; }
    }
}


public static class OwnerIdExtensions
{
    public static OwnerId ToOwnerId(this string subject) => new OwnerId(subject);
}
