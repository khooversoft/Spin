using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Toolbox.Tools;

public static class VerifyBool
{
    [DebuggerStepThrough]
    public static bool Be(this bool subject, bool value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject != value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should be '{value}'", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static bool? Be(this bool? subject, bool? value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject != value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should be '{value}'", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static bool BeTrue(this bool subject, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        return subject.Be(true, because, name);
    }

    [DebuggerStepThrough]
    public static bool? BeTrue(this bool? subject, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        return subject.Be(true, because, name);
    }

    [DebuggerStepThrough]
    public static bool BeFalse(this bool subject, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        return subject.Be(false, because, name);
    }

    [DebuggerStepThrough]
    public static bool? BeFalse(this bool? subject, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        return subject.Be(false, because, name);
    }
}
