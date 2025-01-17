using System.Runtime.CompilerServices;

namespace Toolbox.Tools.Should;

public static class ShouldLongExtensions
{
    public static ShouldContext<long> Should(
        this long subject,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        return new ShouldContext<long>(subject, function, path, lineNumber, name);
    }

    public static ShouldContext<long?> Should(
        this long? subject,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        return new ShouldContext<long?>(subject, function, path, lineNumber, name);
    }

    public static ShouldContext<long> Be(this ShouldContext<long> subject, long value, string? because = null)
    {
        if (subject.Value.Equals(value) == false) subject.ThrowException($"Value is '{subject.Value}' but should be '{value}'", because);
        return subject;
    }

    public static ShouldContext<long?> Be(this ShouldContext<long?> subject, long? value, string? because = null)
    {
        if (subject.Value.Equals(value) == false) subject.ThrowException($"Value is '{subject.Value}' but should be '{value}'", because);
        return subject;
    }

    public static ShouldContext<long> NotBe(this ShouldContext<long> subject, long value, string? because = null)
    {
        if (subject.Value.Equals(value) == true) subject.ThrowException($"Value is '{subject.Value}' but should not be '{value}'", because);
        return subject;
    }

    public static ShouldContext<long?> NotBe(this ShouldContext<long?> subject, long? value, string? because = null)
    {
        if (subject.Value.Equals(value) == true) subject.ThrowException($"Value is '{subject.Value}' but should not be '{value}'", because);
        return subject;
    }
}
