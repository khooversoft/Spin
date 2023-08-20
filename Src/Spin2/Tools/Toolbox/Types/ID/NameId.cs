﻿using Toolbox.Tools;

namespace Toolbox.Types;

public readonly record struct NameId
{
    public NameId(string value)
    {
        IsValid(value).Assert(x => x == true, "Syntax error");
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;

    public static bool operator ==(NameId left, string right) => left.Value.Equals(right);
    public static bool operator !=(NameId left, string right) => !(left == right);
    public static bool operator ==(string left, NameId right) => left.Equals(right.Value);
    public static bool operator !=(string left, NameId right) => !(left == right);

    public static implicit operator NameId(string subject) => new NameId(subject);
    public static implicit operator string(NameId subject) => subject.ToString();

    public static bool IsValid(string subject) => IdPatterns.IsName(subject);
    public static Option<NameId> Create(string subject)
    {
        subject = Uri.UnescapeDataString(subject);
        return IsValid(subject) ? new NameId(subject) : StatusCode.BadRequest;
    }
}


public static class NameIdExtensions
{
    public static string ToUrlEncoding(this NameId subject) => Uri.EscapeDataString(subject.ToString());
}
