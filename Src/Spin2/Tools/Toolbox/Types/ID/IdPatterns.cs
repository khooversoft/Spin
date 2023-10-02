using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

public static class IdPatterns
{
    public static bool StandardCharacterTest(char x) => char.IsLetterOrDigit(x) || x == '-' || x == '.' || x == '$';

    public static bool IsDomain(string? subject) =>
        subject.IsNotEmpty() &&
        subject.Func(x => x.IndexOf("..") < 0) &&
        subject.Func(x => x.Count(y => y == '.') >= 1) &&
        TestStart(subject, x => char.IsLetter(x) || x == '$') &&
        TestMiddle(subject, StandardCharacterTest) &&
        TestEnd(subject, char.IsLetterOrDigit);

    public static bool IsName(string? subject) =>
        subject.IsNotEmpty() &&
        subject.Func(x => x.IndexOf(".") < 0) &&
        TestStart(subject, char.IsLetter) &&
        TestMiddle(subject, StandardCharacterTest) &&
        TestEnd(subject, char.IsLetterOrDigit);

    public static bool IsPath(string? subject) =>
        subject.IsNotEmpty() &&
        TestStart(subject, char.IsLetter) &&
        TestMiddle(subject, x => StandardCharacterTest(x) || x == '@' || x == '/') &&
        TestEnd(subject, char.IsLetterOrDigit);

    public static bool IsPrincipalId(string? subject) =>
        subject.IsNotEmpty() &&
        subject.Split(':') switch
        {
            var s when s.Length == 2 && s[0] != "user" => false,
            var s => s.Last().Split('@') switch
            {
                var v when v.Length != 2 => false,
                var v when v[0].IsEmpty() || v[0].IsEmpty() => false,
                var v => IsName(v[0]) && IsDomain(v[1])
            }
        };

    public static bool IsKeyId(string? subject) =>
        subject.IsNotEmpty() &&
        subject.Split(':') switch
        {
            var s when s.Length != 2 => false,
            var s when s[0] != "kid" => false,
            var s => s.Last().Split('/').Func(x => IsPrincipalId(x[0]) && x.Skip(1).All(x => IsPath(x)))
        };

    public static bool IsContractId(string? subject) => IsSchemaDomainMatch(subject, "contract");
    public static bool IsLeaseId(string? subject) => IsSchemaDomainMatch(subject, "lease");

    public static bool IsAccountId(string? subject) =>
        subject.IsNotEmpty() &&
        subject.Split(':') switch
        {
            var s when s.Length > 2 => false,
            var s => s.Last().Split('/').Func(x => x.Length > 1 && IsDomain(x[0]) && x.Skip(1).All(x => IsPath(x)))
        };

    public static bool IsSchemaDomainMatch(string? subject, string schema) =>
        subject.IsNotEmpty() &&
        subject.Split(':') switch
        {
            var s when s.Length != 2 => false,
            var s when s[0] != schema.NotEmpty() => false,
            var s => s.Last().Split('/').Func(x => x.Length > 1 && IsDomain(x[0]) && x.Skip(1).All(x => IsPath(x)))
        };

    public static bool TestStart(string subject, Func<char, bool> test) => test(subject[0]);

    public static bool TestMiddle(string subject, Func<char, bool> test) => subject switch
    {
        var v when v.Length <= 2 => true,
        var v => v[1..^1].All(x => test(x)),
    };

    public static bool TestEnd(string subject, Func<char, bool> test) => subject switch
    {
        var v when v.Length == 1 => true,
        var v => test(v[^1]),
    };
}
