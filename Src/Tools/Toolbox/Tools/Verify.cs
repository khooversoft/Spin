using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Toolbox.Extensions;

namespace Toolbox.Tools;

public static class Verify
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
    public static T Assert<T>(
            this T subject,
            Func<T, bool> test,
            string message,
            [CallerMemberName] string function = "",
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerArgumentExpression("subject")] string name = ""
        )
    {
        if (test(subject)) return subject;
        message.NotEmpty();

        message += $", {name}, " + FormatCaller(function, path, lineNumber);
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
    public static T Assert<T>(
            this T subject,
            Func<T, bool> test,
            Func<T, string> getMessage,
            [CallerMemberName] string function = "",
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerArgumentExpression("subject")] string name = ""
        )
    {
        if (test(subject)) return subject;

        getMessage.NotNull();
        string msg = getMessage(subject) + $", {name}, " + FormatCaller(function, path, lineNumber);
        throw new ArgumentException(msg);
    }

    /// <summary>
    /// Assert test and throw exception with message
    /// </summary>
    /// <typeparam name="T">type of exception</typeparam>
    /// <param name="test">test</param>
    /// <param name="message">exception message optional</param>
    [DebuggerStepThrough]
    public static T Assert<T, TException>(
            this T subject,
            Func<T, bool> test,
            Func<T, string> getMessage,
            [CallerMemberName] string function = "",
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerArgumentExpression("subject")] string name = ""
        ) where TException : Exception
    {
        if (test(subject)) return subject;
        getMessage.NotNull();

        string msg = getMessage(subject) + $", {name}, " + FormatCaller(function, path, lineNumber);

        throw (Exception)Activator.CreateInstance(typeof(TException), msg)!;
    }

    /// <summary>
    /// Verify subject is not null or default
    /// </summary>
    /// <typeparam name="T">subject type</typeparam>
    /// <param name="subject">subject</param>
    /// <param name="name">name of subject or message</param>
    /// <returns>subject</returns>
    [DebuggerStepThrough]
    [return: NotNull]
    public static T NotNull<T>(
            [NotNull] this T subject,
            string? message = null,
            [CallerMemberName] string function = "",
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerArgumentExpression("subject")] string name = ""
        )
    {
        if (subject == null || EqualityComparer<T>.Default.Equals(subject, default!))
        {
            string msg = message ?? "Null object";
            msg += $", {name}, {FormatCaller(function, path, lineNumber)}";
            throw new ArgumentNullException(msg);
        }

        return subject;
    }

    /// <summary>
    /// Verify subject is not null or default
    /// </summary>
    /// <typeparam name="T">subject type</typeparam>
    /// <param name="subject">subject</param>
    /// <param name="name">name of subject or message</param>
    /// <returns>subject</returns>
    [DebuggerStepThrough]
    public static T BeNull<T>(
            [NotNull] this T subject,
            string? message = null,
            [CallerMemberName] string function = "",
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerArgumentExpression("subject")] string name = ""
        )
    {
        if (subject != null || !EqualityComparer<T>.Default.Equals(subject, default!))
        {
            string msg = message ?? "Not null object";
            msg += $", {name}, {FormatCaller(function, path, lineNumber)}";
            throw new ArgumentNullException(msg);
        }

        return subject;
    }

    /// <summary>
    /// Verify subject is not null or empty
    /// </summary>
    /// <param name="subject">subject</param>
    /// <param name="name">name of subject or message</param>
    /// <returns>subject</returns>
    [DebuggerStepThrough]
    [return: NotNull]
    public static string NotEmpty(
            [NotNull] this string? subject,
            string? message = null,
            [CallerMemberName] string function = "",
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerArgumentExpression("subject")] string name = ""
        )
    {
        if (subject.IsEmpty())
        {
            string msg = message ?? "Empty or null string";
            msg += $", {name}, {FormatCaller(function, path, lineNumber)}";
            throw new ArgumentNullException(msg);
        }

        return subject;
    }

    [DebuggerStepThrough]
    public static string FormatCaller(string function, string path, int lineNumber) => $"Function={function}, File={path}, LineNumber={lineNumber}";
}
