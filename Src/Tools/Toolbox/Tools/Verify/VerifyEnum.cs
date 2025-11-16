using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Toolbox.Tools;

public static class VerifyEnum
{
    [DebuggerStepThrough]
    public static T Be<T>(this T subject, T value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
        where T : struct, Enum
    {
        if (!subject.Equals(value)) throw new ArgumentException(Verify.FormatException($"Value is '{subject}', should be '{value}'", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static T? Be<T>(this T? subject, T? value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
        where T : struct, Enum
    {
        if (!subject.Equals(value)) throw new ArgumentException(Verify.FormatException($"Value is '{subject}', should be '{value}'", because));
        return subject;
    }
}
