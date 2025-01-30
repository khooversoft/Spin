using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;

namespace Toolbox.Tools.Should;

[DebuggerStepThrough]
public static class ShouldEnumerable
{
    [return: NotNull]
    public static ShouldContext<IEnumerable<T>> Should<T>(
            [NotNull] this IEnumerable<T> subject,
            [CallerMemberName] string function = "",
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerArgumentExpression("subject")] string name = ""
        )
    {
        return new ShouldContext<IEnumerable<T>>(subject, function, path, lineNumber, name);
    }

    public static ShouldContext<IEnumerable<T>> BeEquivalent<T>(this ShouldContext<IEnumerable<T>> subject, IEnumerable<T> value, string? because = null)
    {
        var result = subject.Value.IsEquivalent(value);
        if (!result) subject.ThrowException($"Value is not equivalent to '{value}'", because);

        return subject;
    }
}
