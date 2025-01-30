using System.Diagnostics;
using System.Runtime.CompilerServices;
using Toolbox.Extensions;

namespace Toolbox.Tools.Should;

[DebuggerStepThrough]
public static class ShouldStringExtensions
{
    public static ShouldContext<string?> Should(
        this string? subject,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        return new ShouldContext<string?>(subject, function, path, lineNumber, name);
    }

    public static ShouldContext<string?> Be(this ShouldContext<string?> subject, string? value, string? because = null)
    {
        if (subject.Value?.Equals(value) == false) subject.ThrowException($"Value is '{subject.Value}' but should be '{value}'", because);
        return subject;
    }

    public static ShouldContext<string?> NotBe(this ShouldContext<string?> subject, string? value, string? because = null)
    {
        if (subject.Value == value || subject.Value?.Equals(value) == true) subject.ThrowException($"Value is '{subject.Value}' but should not be '{value}'", because);
        return subject;
    }

    public static ShouldContext<string?> BeEmpty(this ShouldContext<string?> subject, string? because = null)
    {
        if (subject.Value.IsNotEmpty()) subject.ThrowException("Value is not empty", because);
        return subject;
    }

    public static ShouldContext<string?> NotBeEmpty(this ShouldContext<string?> subject, string? because = null)
    {
        if (subject.Value.IsEmpty()) subject.ThrowException("Value is empty", because);
        return subject;
    }
}
