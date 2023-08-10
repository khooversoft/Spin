using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

    public string Id => _details.PrincipalId;
    [JsonIgnore] public string Name => _details.Name;
    [JsonIgnore] public string Domain => _details.Domain;
    public override string ToString() => _details.PrincipalId;

    public bool Equals(PrincipalId? obj) => obj is PrincipalId document && Id == document.Id;
    public override int GetHashCode() => HashCode.Combine(Id);

    public static bool IsValid(string subject) => IdPatterns.IsPrincipalId(subject);


    public static Option<ParseDetails> Parse(string principalId)
    {
        if (!IsValid(principalId)) return StatusCode.BadRequest;

        string[] parts = principalId.Split('@').Assert(x => x.Length == 2, "Failed");

        return new ParseDetails
        {
            PrincipalId = principalId,
            Name = parts[0],
            Domain = parts[1],
        };
    }

    public static Option<PrincipalId> CreateIfValid(string principalId) => Parse(principalId) switch
    {
        var v when v.IsError() => v.ToOptionStatus<PrincipalId>(),
        var v => new PrincipalId(v.Return()),
    };

    public static bool operator ==(PrincipalId left, string right) => left.Id.Equals(right);
    public static bool operator !=(PrincipalId left, string right) => !(left == right);
    public static bool operator ==(string left, PrincipalId right) => left.Equals(right.Id);
    public static bool operator !=(string left, PrincipalId right) => !(left == right);

    public static implicit operator PrincipalId(string subject) => new PrincipalId(subject);
    public static implicit operator string(PrincipalId subject) => subject.ToString();

    public readonly record struct ParseDetails
    {
        public string PrincipalId { get; init; }
        public string Name { get; init; }
        public string Domain { get; init; }
    }
}
