using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Toolbox.Tools;

public static class VerifyBe
{
    [DebuggerStepThrough]
    [return: NotNullIfNotNull(nameof(subject))]
    public static T? Be<T>(this T? subject, T? value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
        where T : class
    {
        if (subject is null && value is null) return subject;

        if (!(subject?.Equals(value) ?? false))
            throw new ArgumentException(Verify.FormatException($"Value is '{subject}', should be '{value}'", because));

        return subject;
    }
}
