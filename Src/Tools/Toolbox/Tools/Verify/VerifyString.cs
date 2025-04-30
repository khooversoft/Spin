using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools;

public static class VerifyString
{
    [return: NotNullIfNotNull(nameof(subject))]
    public static string? Be(
        this string? subject,
        string? value,
        string? because = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        var location = new CodeLocation(function, path, lineNumber, name);

        if (subject != value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}', should be '{value}'", because));
        return subject;
    }
    
    [return: NotNullIfNotNull(nameof(subject))]
    public static string? NotBe(
        this string? subject,
        string? value,
        string? because = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        var location = new CodeLocation(function, path, lineNumber, name);

        if (subject == value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}', should NOT be '{value}'", because));
        return subject;
    }

    [return: NotNullIfNotNull(nameof(subject))]
    public static string? BeEmpty(
        this string? subject,
        string? because = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        var location = new CodeLocation(function, path, lineNumber, name);

        if (subject.IsNotEmpty()) throw new ArgumentException(Verify.FormatException($"Value is '{subject}', should be Empty", because));
        return subject;
    }
}
