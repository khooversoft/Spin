using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Toolbox.Tools;

public static class VerifyDecimal
{
    [DebuggerStepThrough]
    public static decimal Be(this decimal subject, decimal value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject != value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should be '{value}'", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static decimal? Be(this decimal? subject, decimal? value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject != value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should be '{value}'", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static decimal NotBe(this decimal subject, decimal value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject == value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should NOT be '{value}'", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static decimal? NotBe(this decimal? subject, decimal? value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject == value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should NOT be '{value}'", because));
        return subject;
    }
}
