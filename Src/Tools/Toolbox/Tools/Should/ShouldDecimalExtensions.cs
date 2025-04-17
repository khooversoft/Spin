using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Toolbox.Tools.Should;

[DebuggerStepThrough]
public static class ShouldDecimalExtensions
{
    public static ShouldContext<decimal> Should(
        this decimal subject,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        return new ShouldContext<decimal>(subject, function, path, lineNumber, name);
    }

    public static ShouldContext<decimal?> Should(
        this decimal? subject,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        return new ShouldContext<decimal?>(subject, function, path, lineNumber, name);
    }

    public static ShouldContext<decimal> Be(this ShouldContext<decimal> subject, decimal value, string? because = null)
    {
        if (subject.Value.Equals(value) == false) subject.ThrowException($"Value is '{subject.Value}' but should be '{value}'", because);
        return subject;
    }

    public static ShouldContext<decimal?> Be(this ShouldContext<decimal?> subject, decimal? value, string? because = null)
    {
        if (subject.Value.Equals(value) == false) subject.ThrowException($"Value is '{subject.Value}' but should be '{value}'", because);
        return subject;
    }

    public static ShouldContext<decimal> NotBe(this ShouldContext<decimal> subject, decimal value, string? because = null)
    {
        if (subject.Value.Equals(value) == true) subject.ThrowException($"Value is '{subject.Value}' but should not be '{value}'", because);
        return subject;
    }

    public static ShouldContext<decimal?> NotBe(this ShouldContext<decimal?> subject, decimal? value, string? because = null)
    {
        if (subject.Value.Equals(value) == true) subject.ThrowException($"Value is '{subject.Value}' but should not be '{value}'", because);
        return subject;
    }
}
