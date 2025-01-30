using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Toolbox.Tools.Should;

[DebuggerStepThrough]
public static class ShouldActionExtensions
{
    public static ShouldContext<Action> Should(
        this Action subject,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        return new ShouldContext<Action>(subject, function, path, lineNumber, name);
    }

    public static ShouldContext<Action> NotThrow(this ShouldContext<Action> subject, string? because = null)
    {
        try
        {
            subject.Value.Invoke();
        }
        catch (Exception ex)
        {
            subject.ThrowException($"Exception was thrown", because, ex);
        }

        return subject;
    }

    public static ShouldContext<Action> Throw(this ShouldContext<Action> subject, string? because = null)
    {
        try
        {
            subject.Value.Invoke();
        }
        catch
        {
            return subject;
        }

        subject.ThrowException($"Exception was not thrown", because);
        throw new UnreachableException();
    }

    public static ShouldContext<Action> Throw<TException>(this ShouldContext<Action> subject, string? because = null)
        where TException : Exception
    {
        try
        {
            subject.Value.Invoke();
        }
        catch (Exception ex)
        {
            bool isMatch = ex.GetType() == typeof(TException) || ex.GetType().IsSubclassOf(typeof(TException));
            if (!isMatch) subject.ThrowException($"Exception was not thrown", because);
            return subject;
        }

        subject.ThrowException($"Exception was not thrown", because);
        throw new UnreachableException();
    }
}
