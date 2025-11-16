using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Toolbox.Extensions;

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
        catch (Exception ex)
        {
            throw new InvalidOperationException("oops - not expected", ex);
        }

        throw new ArgumentException(Verify.FormatException($"Should throw ex={typeof(TException).Name}", because));
    }

    public static async Task ThrowAsync<TException>(Func<Task> action, string? because = null) where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException)
        {
            return;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("oops - not expected", ex);
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
    public static T NotNull<T>([NotNull] this T subject, string? message = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject != null && !EqualityComparer<T>.Default.Equals(subject, default!)) return subject;
        message ??= $"{name} is Null";
        throw new ArgumentNullException(message);
    }

    /// <summary>
    /// Verify subject is not null or default
    /// </summary>
    /// <typeparam name="T">subject type</typeparam>
    /// <param name="subject">subject</param>
    /// <param name="name">name of subject or message</param>
    /// <returns>subject</returns>
    [DebuggerStepThrough]
    public static T BeNull<T>([NotNull] this T subject, string? message = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject == null && EqualityComparer<T>.Default.Equals(subject, default!)) return subject;
        message ??= $"{name} not Null";
        throw new ArgumentNullException(message);
    }

    /// <summary>
    /// Verify subject is not null or empty
    /// </summary>
    /// <param name="subject">subject</param>
    /// <param name="name">name of subject or message</param>
    /// <returns>subject</returns>
    [DebuggerStepThrough]
    [return: NotNull]
    public static string NotEmpty([NotNull] this string? subject, string? message = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject.IsNotEmpty()) return subject;
        message ??= $"{name} is Empty or null";
        throw new ArgumentNullException(message);
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
