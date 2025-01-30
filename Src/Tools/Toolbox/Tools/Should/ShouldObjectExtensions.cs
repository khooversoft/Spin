using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Toolbox.Tools.Should;

[DebuggerStepThrough]
public static class ShouldObjectExtensions
{
    public static ShouldContext<object?> Should(
        this object? subject,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        return new ShouldContext<object?>(subject, function, path, lineNumber, name);
    }

    public static ShouldContext<object?> Be(this ShouldContext<object?> subject, object? value, string? because = null)
    {
        if (subject.Value?.Equals(value) == false) subject.ThrowException($"Value is '{subject.Value}' but should be '{value}'", because);
        return subject;
    }

    public static ShouldContext<object?> NotBe(this ShouldContext<object?> subject, object? value, string? because = null)
    {
        if (subject.Value?.Equals(value) == true) subject.ThrowException($"Value is '{subject.Value}' but should not be '{value}'", because);
        return subject;
    }
}
