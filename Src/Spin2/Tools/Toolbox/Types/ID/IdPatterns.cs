using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;

namespace Toolbox.Types;

public static class IdPatterns
{
    // Schema = AZaz09:AZaz09-_.$:AZaz09
    public static bool IsSchema(string subject) => TestPattern(
        subject,
        char.IsLetter,
        x => char.IsLetterOrDigit(x) || x == '-' || x == '_' || x == '.' || x == '$',
        char.IsLetterOrDigit
        );

    public static bool IsTenant(string subject) =>
        subject.IsNotEmpty() &&
        subject.Count(x => x == '.') < 2 &&
        TestPattern(
            subject,
            char.IsLetter,
            x => char.IsLetterOrDigit(x) || x == '-' || x == '_' || x == '.' || x == '$',
            char.IsLetterOrDigit
        );

    public static bool IsName(string subject) =>
        subject.IsNotEmpty() &&
        subject.Func(x => x.IndexOf("..") < 0) &&
        TestPattern(
            subject,
            char.IsLetter,
            x => char.IsLetterOrDigit(x) || x == '-' || x == '_' || x == '$' || x == '.',
            char.IsLetterOrDigit
            );

    public static bool IsPath(string subject) => TestPattern(
        subject,
        char.IsLetter,
        x => char.IsLetterOrDigit(x) || x == '-' || x == '_' || x == '$' || x == '@' || x == '.',
        char.IsLetterOrDigit
        );

    public static bool IsPrincipalId(string subject) =>
        subject.IsNotEmpty() &&
        subject.Split('@') switch
        {
            var v when v.Length != 2 => false,
            var v when v[0].IsEmpty() || v[0].IsEmpty() => false,
            var v => IsName(v[0]) && IsTenant(v[1])
        };

    public static bool TestPattern(string subject, Func<char, bool> start, Func<char, bool> middle, Func<char, bool> end) =>
        subject.IsNotEmpty() &&
        subject switch
        {
            var v when v.Length < 2 => start(v[0]),
            var v when v.Length == 2 => start(v[0]) && end(v[1]),

            var v => start(v[0]) &&
                end(v[^1]) &&
                v[1..^1].All(x => middle(x)),
        };
}
