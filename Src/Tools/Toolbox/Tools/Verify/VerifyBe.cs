using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Toolbox.Types;

namespace Toolbox.Tools;

public static class VerifyBe
{
    [return: NotNullIfNotNull(nameof(subject))]
    public static T Be<T>(
        this T subject,
        T value,
        string? because = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        ) where T : struct
    {
        var location = new CodeLocation(function, path, lineNumber, name);

        if (subject.Equals(value) == false) throw new ArgumentException(Verify.FormatException($"Value is '{subject}', should be '{value}'", because));
        return subject;
    }
}
