using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Toolbox.Tools;

public static class VerifyAssert
{
    /// <summary>
    /// Verify state
    /// </summary>
    /// <typeparam name="T">any type</typeparam>
    /// <param name="subject">subject</param>
    /// <param name="test">test func</param>
    /// <param name="message">message</param>
    /// <returns>subject</returns>
    [DebuggerStepThrough]
    public static T Assert<T>(this T subject, Func<T, bool> test, string? message = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (test(subject)) return subject;
        message ??= $"Assertion failed for {name}";
        throw new ArgumentException(message);
    }

    /// <summary>
    /// Verify state
    /// </summary>
    /// <typeparam name="T">any type</typeparam>
    /// <param name="subject">subject</param>
    /// <param name="test">test func</param>
    /// <param name="getMessage">get message</param>
    /// <returns>subject</returns>
    [DebuggerStepThrough]
    public static T Assert<T>(this T subject, Func<T, bool> test, Func<T, string?> getMessage, [CallerArgumentExpression("subject")] string name = "")
    {
        if (test(subject)) return subject;
        getMessage.NotNull();
        throw new ArgumentException(getMessage(subject));
    }
}
