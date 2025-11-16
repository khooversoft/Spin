using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Toolbox.Tools;

public static class VerifyDateTime
{
    [DebuggerStepThrough]
    public static DateTime Be(this DateTime subject, DateTime value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject != value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should be '{value}'", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static DateTime? Be(this DateTime? subject, DateTime? value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject != value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should be '{value}'", because));
        return subject;
    }
}
