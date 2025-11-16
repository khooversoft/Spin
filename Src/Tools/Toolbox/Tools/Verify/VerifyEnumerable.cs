using System.Diagnostics;
using System.Runtime.CompilerServices;
using Toolbox.Extensions;

namespace Toolbox.Tools;

public static class VerifyEnumerable
{
    [DebuggerStepThrough]
    public static IEnumerable<T> BeEquivalent<T>(
        this IEnumerable<T> subject,
        IEnumerable<T> value,
        string? because = null,
        IEqualityComparer<T>? comparer = null,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        var result = subject.IsEquivalent(value, comparer);
        if (!result) throw new ArgumentException(Verify.FormatException($"Subject is not equivalent to value", because));

        return subject;
    }
}
