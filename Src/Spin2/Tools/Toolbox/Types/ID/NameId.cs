using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

public readonly record struct NameId
{
    public NameId(string name) => Value = name.Assert(x => IsValid(x), Syntax);

    public const string Syntax = "Valid characters are a-z A-Z 0-9 . $ @ - _ *";

    public string Value { get; }

    public override string ToString() => Value;

    public static bool IsValid(string subject) => IdPatterns.IsName(subject);

    public static bool operator ==(NameId left, string right) => left.Value.Equals(right);
    public static bool operator !=(NameId left, string right) => !(left == right);
    public static bool operator ==(string left, NameId right) => left.Equals(right.Value);
    public static bool operator !=(string left, NameId right) => !(left == right);

    public static implicit operator NameId(string subject) => new NameId(subject);
    public static implicit operator string(NameId subject) => subject.ToString();

    public static string Verify(string id) => id.Action(x => NameId.IsValid(x).Assert($"{x} is not valid name id"));
    public static Option<NameId> CreateIfValid(string id) => IsValid(id) ? new NameId(id) : StatusCode.BadRequest;
}


public static class NameIdExtensions
{
    public static Option<NameId> ToNameIdIfValid(this string subject, ScopeContextLocation location)
    {
        var option = NameId.CreateIfValid(subject);
        if (option.IsError()) location.LogError("NameId is not valid, NameId={NameId}, error={error}", subject, option.Error);
        return option;
    }

    public static string ToUrlEncoding(this NameId subject) => Uri.EscapeDataString(subject.ToString());
}
