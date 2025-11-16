using System.Diagnostics;
using System.Runtime.CompilerServices;
using Toolbox.Extensions;

namespace Toolbox.Tools;

public static class VerifyInt
{
    [DebuggerStepThrough]
    public static int Be(this int subject, int value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject != value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should be '{value}'", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static int? Be(this int? subject, int? value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject != value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should be '{value}'", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static int NotBe(this int subject, int value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject == value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should NOT be '{value}'", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static int? NotBe(this int? subject, int? value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject == value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should NOT be '{value}'", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static int? BeWithinPercentage(this int subject, int target, double percentage, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (!subject.IsWithinPercentage(target, percentage))
        {
            throw new ArgumentException(Verify.FormatException($"Value is '{subject}' is not within percentage {percentage} of {target}", because));
        }

        return subject;
    }
}
