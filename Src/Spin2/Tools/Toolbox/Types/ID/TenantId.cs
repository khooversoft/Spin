using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

public readonly record struct TenantId
{
    public TenantId(string name) => Value = name.Assert(x => IsValid(x), "Syntax errors");

    public string Value { get; }
    public override string ToString() => Value;

    public static bool operator ==(TenantId left, string right) => left.Value.Equals(right);
    public static bool operator !=(TenantId left, string right) => !(left == right);
    public static bool operator ==(string left, TenantId right) => left.Equals(right.Value);
    public static bool operator !=(string left, TenantId right) => !(left == right);

    public static implicit operator TenantId(string subject) => new TenantId(subject);
    public static implicit operator string(TenantId subject) => subject.ToString();

    public static bool IsValid(string subject) => IdPatterns.IsTenant(subject);
    public static Option<TenantId> CreateIfValid(string id) => IsValid(id) ? new TenantId(id) : StatusCode.BadRequest;
}
