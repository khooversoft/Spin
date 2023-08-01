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
    public NameId(string name)
    {
        Name = name.Assert(x => IsValid(x), Syntax);
    }

    public const string Syntax = "Valid characters are a-z A-Z 0-9 . $ @ - _ *";

    public string Name { get; }

    public override string ToString() => Name;

    public static bool IsValid(string subject) => subject.IsNotEmpty() && subject.All(x => IsCharacterValid(x));

    public static bool IsCharacterValid(char ch) =>
        char.IsLetterOrDigit(ch) || ch == '.' || ch == '-' || ch == '$' || ch == '@' || ch == '_' || ch == '*' || ch == ':';

    public static bool operator ==(NameId left, string right) => left.Name.Equals(right);
    public static bool operator !=(NameId left, string right) => !(left == right);
    public static bool operator ==(string left, NameId right) => left.Equals(right.Name);
    public static bool operator !=(string left, NameId right) => !(left == right);

    public static string Verify(string id) => id.Action(x => NameId.IsValid(x).Assert($"{x} is not valid name id"));
}
