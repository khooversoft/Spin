using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools;

public static class Verify
{
    public static void Throw<TException>(Action action, string? because = null) where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }

        throw new ArgumentException(Verify.FormatException($"Should throw ex={typeof(TException).Name}", because));
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
        if (subject != null && !EqualityComparer<T>.Default.Equals(subject, default!)) return subject;

        var location = new CodeLocation(function, path, lineNumber, name);
        var structLine = StructureLineBuilder.Start()
            .Add(message ?? "Null object")
            .Add(location)
            .Build()
            .Format();

        throw new ArgumentNullException(structLine);
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
        if (subject == null && EqualityComparer<T>.Default.Equals(subject, default!)) return subject;

        var location = new CodeLocation(function, path, lineNumber, name);
        var structLine = StructureLineBuilder.Start()
            .Add(message ?? "Not null object")
            .Add(location)
            .Build()
            .Format();

        throw new ArgumentNullException(structLine);
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
        if (subject.IsNotEmpty()) return subject;

        var location = new CodeLocation(function, path, lineNumber, name);
        var structLine = StructureLineBuilder.Start()
            .Add(message ?? "Empty or null string")
            .Add(location)
            .Build()
            .Format();

        throw new ArgumentNullException(structLine);
    }

    internal static string FormatException(string message, string? because = null)
    {
        var sb = new StringBuilder(message.NotEmpty());

        if (because.IsNotEmpty())
        {
            sb.Append(", because=");
            sb.Append(because);
        }

        return sb.ToString();
    }
}
