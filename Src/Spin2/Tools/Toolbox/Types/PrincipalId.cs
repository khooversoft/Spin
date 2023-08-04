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

public sealed record PrincipalId
{
    private ParseDetails _details;

    [JsonConstructor]
    public PrincipalId(string id) => _details = Parse(id).ThrowOnError().Return();
    public PrincipalId(ParseDetails parseDetails) => _details = parseDetails.NotNull();

    public string Id => _details.OwnerId;
    [JsonIgnore] public string Name => _details.Name;
    [JsonIgnore] public string Domain => _details.Domain;
    public override string ToString() => _details.OwnerId;

    public static bool IsValid(string subject) => Parse(subject).StatusCode.IsOk();

    public bool Equals(PrincipalId? obj) => obj is PrincipalId document && Id == document.Id;
    public override int GetHashCode() => HashCode.Combine(Id);

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

    public static Option<PrincipalId> CreateIfValid(string ownerId, ScopeContext context)
    {
        Option<ParseDetails> parseDetails = Parse(ownerId).LogResult(context.Location());
        if (parseDetails.IsError()) return parseDetails.ToOptionStatus<PrincipalId>();

        return new PrincipalId(parseDetails.Return());
    }

    private static bool IsPrefixCharacterValid(char ch) => char.IsLetterOrDigit(ch) || ch == '.' || ch == '-' || ch == '@' || ch == '_';
    private static bool IsDomainCharacterValid(char ch) => char.IsLetterOrDigit(ch) || ch == '-' || ch == '.';
    private static bool InvalidStartOrEnd(char ch) => ch == '.' || ch == '-' || ch == '_';

    public static bool operator ==(PrincipalId left, string right) => left.Id.Equals(right);
    public static bool operator !=(PrincipalId left, string right) => !(left == right);
    public static bool operator ==(string left, PrincipalId right) => left.Equals(right.Id);
    public static bool operator !=(string left, PrincipalId right) => !(left == right);

    public static implicit operator PrincipalId(string subject) => new PrincipalId(subject);
    public static implicit operator string(PrincipalId subject) => subject.ToString();

    public readonly record struct ParseDetails
    {
        public string OwnerId { get; init; }
        public string Name { get; init; }
        public string Domain { get; init; }
    }
}


public static class OwnerIdExtensions
{
    public static PrincipalId ToOwnerId(this string subject) => new PrincipalId(subject);
}
