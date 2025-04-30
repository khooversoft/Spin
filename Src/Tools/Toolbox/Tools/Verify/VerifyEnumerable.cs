using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools;

public static class VerifyEnumerable
{
    public static IEnumerable<T> BeEquivalent<T>(
        this IEnumerable<T> subject,
        IEnumerable<T> value,
        string? because = null,
        IEqualityComparer<T>? comparer = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        var location = new CodeLocation(function, path, lineNumber, name);

        var result = subject.IsEquivalent(value, comparer);
        if (!result ) throw new ArgumentException(Verify.FormatException($"Subject is not equivalent to value", because));

        return subject;
    }
}
