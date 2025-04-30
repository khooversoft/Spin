using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Types;

namespace Toolbox.Tools;

public static class VerifyBool
{
    public static bool Be(
        this bool subject,
        bool value,
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

    public static bool? Be(
        this bool? subject,
        bool? value,
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

    public static bool BeTrue(
        this bool subject,
        string? because = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        return subject.Be(true, because, function, path, lineNumber, name);
    }

    public static bool? BeTrue(
        this bool? subject,
        string? because = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        return subject.Be(true, because, function, path, lineNumber, name);
    }

    public static bool BeFalse(
        this bool subject,
        string? because = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        return subject.Be(false, because, function, path, lineNumber, name);
    }

    public static bool? BeFalse(
        this bool? subject,
        string? because = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        return subject.Be(false, because, function, path, lineNumber, name);
    }
}
