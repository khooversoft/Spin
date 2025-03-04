using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Toolbox.Tools.Should;

[DebuggerStepThrough]
public static class ShouldDoubleExtensions
{
    public static ShouldContext<double> Should(
        this float subject,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        return new ShouldContext<double>(subject, function, path, lineNumber, name);
    }

    public static ShouldContext<double?> Should(
        this float? subject,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        return new ShouldContext<double?>(subject, function, path, lineNumber, name);
    }

    public static ShouldContext<double> Be(this ShouldContext<double> subject, double value, string? because = null)
    {
        if (subject.Value.Equals(value) == false) subject.ThrowException($"Value is '{subject.Value}' but should be '{value}'", because);
        return subject;
    }

    public static ShouldContext<double?> Be(this ShouldContext<double?> subject, double? value, string? because = null)
    {
        if (subject.Value.Equals(value) == false) subject.ThrowException($"Value is '{subject.Value}' but should be '{value}'", because);
        return subject;
    }

    public static ShouldContext<double> NotBe(this ShouldContext<double> subject, double value, string? because = null)
    {
        if (subject.Value.Equals(value) == true) subject.ThrowException($"Value is '{subject.Value}' but should be '{value}'", because);
        return subject;
    }

    public static ShouldContext<double?> NotBe(this ShouldContext<double?> subject, double? value, string? because = null)
    {
        if (subject.Value.Equals(value) == true) subject.ThrowException($"Value is '{subject.Value}' but should be '{value}'", because);
        return subject;
    }
}
