using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

/// <summary>
/// Key ID used for public and private keys, also in JWT
/// 
/// Id = {principalId}[/{path},...]
/// 
/// </summary>
public readonly record struct KeyId
{
    [JsonConstructor]
    public KeyId(string value)
    {
        IsValid(value).Assert(x => x == true, "Syntax error");
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;

    public static bool operator ==(KeyId left, string right) => left.Value.Equals(right);
    public static bool operator !=(KeyId left, string right) => !(left == right);
    public static bool operator ==(string left, KeyId right) => left.Equals(right.Value);
    public static bool operator !=(string left, KeyId right) => !(left == right);

    public static implicit operator KeyId(string subject) => new KeyId(subject);
    public static implicit operator string(KeyId subject) => subject.ToString();

    public static bool IsValid(string subject) => IdPatterns.IsKeyId(subject);

    public static Option<KeyId> Create(PrincipalId principalId, string? name = null)
    {
        string id = name.ToNullIfEmpty() switch
        {
            string v => $"{principalId}/{v}",
            _ => principalId.ToString(),
        };

        if (!ObjectId.IsValid(id)) return StatusCode.BadRequest;

        return new KeyId(id);
    }
}


public static class KeyIdExtensions
{
    public static string ToUrlEncoding(this KeyId subject) => Uri.EscapeDataString(subject.ToString());

    public static PrincipalId GetPrincipalId(this KeyId subject) => subject.GetDetails().PrincipalId;

    public static (PrincipalId PrincipalId, string? Path) GetDetails(this KeyId subject)
    {
        var list = subject.NotNull().Value.Split('/');
        return (list.First(), list.Skip(1).Join('/').ToNullIfEmpty());
    }
}
