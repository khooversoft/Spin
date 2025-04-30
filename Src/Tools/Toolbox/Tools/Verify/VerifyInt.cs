using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Types;

namespace Toolbox.Tools;

public static class VerifyInt
{
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
}
