using System.Runtime.CompilerServices;
using Toolbox.Types;

namespace Toolbox.Tools;

public static class VerifyOption
{
    public static Option Be(
        this Option subject,
        StatusCode value,
        string? because = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        var location = new CodeLocation(function, path, lineNumber, name);

        if (subject.StatusCode != value) throw new ArgumentException(Verify.FormatException($"Value is '{subject.StatusCode}' should be '{value}'", because));
        return subject;
    }

    public static Option NotBe(
        this Option subject,
        StatusCode value,
        string? because = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        var location = new CodeLocation(function, path, lineNumber, name);

        if (subject.StatusCode == value) throw new ArgumentException(Verify.FormatException($"Value is '{subject.StatusCode}' NOT should be '{value}'", because));
        return subject;
    }

    public static Option<T> Be<T>(
        this Option<T> subject,
        StatusCode value,
        string? because = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        var location = new CodeLocation(function, path, lineNumber, name);

        if (subject.StatusCode != value) throw new ArgumentException(Verify.FormatException($"Value is '{subject.StatusCode}' should be '{value}'", because));
        return subject;
    }

    public static Option<T> NotBe<T>(
        this Option<T> subject,
        StatusCode value,
        string? because = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        var location = new CodeLocation(function, path, lineNumber, name);

        if (subject.StatusCode == value) throw new ArgumentException(Verify.FormatException($"Value is '{subject.StatusCode}' NOT should be '{value}'", because));
        return subject;
    }

    public static Option BeOk(
        this Option subject,
        string? because = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        var location = new CodeLocation(function, path, lineNumber, name);

        if (!subject.StatusCode.IsOk()) throw new ArgumentException(Verify.FormatException($"Value is '{subject.StatusCode}' should be OK", because));
        return subject;
    }

    public static Option<T> BeOk<T>(
        this Option<T> subject,
        string? because = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        var location = new CodeLocation(function, path, lineNumber, name);

        if (!subject.StatusCode.IsOk()) throw new ArgumentException(Verify.FormatException($"Value is '{subject.StatusCode}' should be OK", because));
        return subject;
    }

    public static Option BeError(
        this Option subject,
        string? because = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        var location = new CodeLocation(function, path, lineNumber, name);

        if (!subject.StatusCode.IsError()) throw new ArgumentException(Verify.FormatException($"Value is '{subject.StatusCode}' should be OK", because));
        return subject;
    }

    public static Option<T> BeError<T>(
        this Option<T> subject,
        string? because = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        var location = new CodeLocation(function, path, lineNumber, name);

        if (!subject.StatusCode.IsError()) throw new ArgumentException(Verify.FormatException($"Value is '{subject.StatusCode}' should be OK", because));
        return subject;
    }
}
