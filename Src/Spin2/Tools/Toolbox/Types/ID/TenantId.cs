using Toolbox.Tools;

namespace Toolbox.Types;

public readonly record struct TenantId
{
    public TenantId(string value)
    {
        IsValid(value).Assert(x => x == true, "Syntax error");
        Value = value;
    }

    public string Value { get; }
    public override string ToString() => Value;

    public static bool operator ==(TenantId left, string right) => left.Value.Equals(right);
    public static bool operator !=(TenantId left, string right) => !(left == right);
    public static bool operator ==(string left, TenantId right) => left.Equals(right.Value);
    public static bool operator !=(string left, TenantId right) => !(left == right);

    public static implicit operator TenantId(string subject) => new TenantId(subject);
    public static implicit operator string(TenantId subject) => subject.ToString();

    public static bool IsValid(string subject) => IdPatterns.IsTenant(subject);
    public static Option<TenantId> Create(string subject)
    {
        subject = Uri.UnescapeDataString(subject);
        return IsValid(subject) ? new TenantId(subject) : StatusCode.BadRequest;
    }
}


public static class TenantIdExtensions
{
    public static string ToUrlEncoding(this TenantId subject) => Uri.EscapeDataString(subject.ToString());
}