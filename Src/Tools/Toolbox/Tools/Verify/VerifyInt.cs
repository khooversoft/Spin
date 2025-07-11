using System.Diagnostics;
using System.Runtime.CompilerServices;
using Toolbox.Types;

namespace Toolbox.Tools;

public static class VerifyInt
{
    [DebuggerStepThrough]
    public static int Be(
        this int subject,
        int value,
        string? because = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        var location = new CodeLocation(function, path, lineNumber, name);

        if (subject != value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should be '{value}'", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static int? Be(
        this int? subject,
        int? value,
        string? because = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        var location = new CodeLocation(function, path, lineNumber, name);

        if (subject != value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should be '{value}'", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static int NotBe(
        this int subject,
        int value,
        string? because = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        var location = new CodeLocation(function, path, lineNumber, name);

        if (subject == value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should NOT be '{value}'", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static int? NotBe(
        this int? subject,
        int? value,
        string? because = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        var location = new CodeLocation(function, path, lineNumber, name);

        if (subject == value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should NOT be '{value}'", because));
        return subject;
    }
}
