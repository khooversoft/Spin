using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Toolbox.Extensions;

namespace Toolbox.Tools;

public static class VerifyIn
{
    [DebuggerStepThrough]
    public static T BeIn<T>([DisallowNull] this T subject, IEnumerable<T> values, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        bool found = values.Any(x => subject.Equals(x));
        if (!found)
        {
            var valueStr = values.Select(x => x?.ToString() ?? "null").Join(", ");
            throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should one of '{valueStr}'", because));
        }

        return subject;
    }

    [DebuggerStepThrough]
    public static IEnumerable<T> BeIn<T>(this IEnumerable<T> subject, IEnumerable<T> values, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        var intersectSet = subject.NotNull().Intersect(values.NotNull()).ToArray();
        if (intersectSet.Length == 0)
        {
            var valueStr = values.Select(x => x?.ToString() ?? "null").Join(", ");
            throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should be '{valueStr}'", because));
        }
        return subject;
    }
}
