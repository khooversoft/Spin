using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tokenizer.Token;
using Toolbox.Tools;

namespace Toolbox.Types;

public readonly record struct OwnerId
{
    public OwnerId(string name)
    {
        Id = name.Assert(x => IsValid(x).StatusCode == StatusCode.OK, "Syntax error");
    }

    public string Id { get; }
    public override string ToString() => Id;

    public static Option IsValid(string subject)
    {
        const string invalidEmail = "No valid email syntax";

        if (subject.IsEmpty()) return new Option(StatusCode.BadRequest, "Is empty");


        if (subject.IndexOf("..") >= 0) return new Option(StatusCode.BadRequest, invalidEmail);

        string[] parts = subject.Split('@');
        if (parts.Length != 2) return new Option(StatusCode.BadRequest, invalidEmail);

        // Prefix
        if (parts[0].IsEmpty() || parts[1].IsEmpty()) return new Option(StatusCode.BadRequest, invalidEmail);
        if (!parts[0].All(x => IsPrefixCharacterValid(x))) return new Option(StatusCode.BadRequest, "Invalid name");

        if (InvalidStartOrEnd(parts[0][0]) || InvalidStartOrEnd(parts[0][^1]))
            return new Option(StatusCode.BadRequest, "Invalid start or end character for prefix");


        // Domain
        if (InvalidStartOrEnd(parts[1][0]) || InvalidStartOrEnd(parts[1][^1]))
            return new Option(StatusCode.BadRequest, "Invalid start or end character for domain");

        if (!parts[1].All(x => IsDomainCharacterValid(x))) return new Option(StatusCode.BadRequest, "Invalid domain");

        string[] domainParts = parts[1].Split('.');
        if (domainParts.Length != 2) return new Option(StatusCode.BadRequest, "Invalid domain");

        return new Option(StatusCode.OK);
    }

    private static bool IsPrefixCharacterValid(char ch) =>
        char.IsLetterOrDigit(ch) || ch == '.' || ch == '-' || ch == '@' || ch == '_';

    private static bool IsDomainCharacterValid(char ch) => char.IsLetterOrDigit(ch) || ch == '-' || ch == '.';

    private static bool InvalidStartOrEnd(char ch) => ch == '.' || ch == '-' || ch == '_';

    public static bool operator ==(OwnerId left, string right) => left.Id.Equals(right);
    public static bool operator !=(OwnerId left, string right) => !(left == right);
}
