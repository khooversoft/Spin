using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Toolbox.Extensions;

namespace Toolbox.Tools;

public static class VerifyString
{
    [DebuggerStepThrough]
    [return: NotNullIfNotNull(nameof(subject))]
    public static string? Be(this string? subject, string? value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject != value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}', should be '{value}'", because));
        return subject;
    }

    [DebuggerStepThrough]
    [return: NotNullIfNotNull(nameof(subject))]
    public static string? NotBe(this string? subject, string? value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject == value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}', should NOT be '{value}'", because));
        return subject;
    }

    [DebuggerStepThrough]
    [return: NotNullIfNotNull(nameof(subject))]
    public static string? BeEmpty(this string? subject, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject.IsNotEmpty()) throw new ArgumentException(Verify.FormatException($"Value is '{subject}', should be Empty", because));
        return subject;
    }
}
