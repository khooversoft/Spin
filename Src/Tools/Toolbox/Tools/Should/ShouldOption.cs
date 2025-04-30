using System.Diagnostics;
using System.Runtime.CompilerServices;
using Toolbox.Types;

namespace Toolbox.Tools.Should;

[DebuggerStepThrough]
public static class ShouldOption
{
    public static ShouldContext<Option> Should(
            this Option subject,
            [CallerMemberName] string function = "",
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerArgumentExpression("subject")] string name = ""
        )
    {
        return new ShouldContext<Option>(subject, function, path, lineNumber, name);
    }

    public static ShouldContext<Option<T>> Should<T>(
            this Option<T> subject,
            [CallerMemberName] string function = "",
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerArgumentExpression("subject")] string name = ""
        )
    {
        return new ShouldContext<Option<T>>(subject, function, path, lineNumber, name);
    }

    public static ShouldContext<Option> BeOk(this ShouldContext<Option> subject, string? because = null)
    {
        if (subject.Value.IsOk() == false) subject.ThrowException($"Value is '{subject.Value.ToString()}' but should be IsOk", because);
        return subject;
    }

    public static ShouldContext<Option> BeError(this ShouldContext<Option> subject, string? because = null)
    {
        if (subject.Value.IsError() == false) subject.ThrowException($"Value is '{subject.Value.ToString()}' but should be IsError", because);
        return subject;
    }

    public static ShouldContext<Option> BeNotFound(this ShouldContext<Option> subject, string? because = null)
    {
        if (subject.Value.IsNotFound() == false) subject.ThrowException($"Value is '{subject.Value.ToString()}' but should be IsNotFound", because);
        return subject;
    }

    public static ShouldContext<Option> BeConflict(this ShouldContext<Option> subject, string? because = null)
    {
        if (subject.Value.IsConflict() == false) subject.ThrowException($"Value is '{subject.Value.ToString()}' but should be IsConflict", because);
        return subject;
    }

    public static ShouldContext<Option> BeBadRequest(this ShouldContext<Option> subject, string? because = null)
    {
        if (subject.Value.IsBadRequest() == false) subject.ThrowException($"Value is '{subject.Value.ToString()}' but should be IsBadRequest", because);
        return subject;
    }

    public static ShouldContext<Option<T>> BeOk<T>(this ShouldContext<Option<T>> subject, string? because = null)
    {
        if (subject.Value.IsOk() == false) subject.ThrowException($"Value is '{subject.Value.ToString()}' but should be IsOk", because);
        return subject;
    }
    public static ShouldContext<Option<T>> BeError<T>(this ShouldContext<Option<T>> subject, string? because = null)
    {
        if (subject.Value.IsError() == false) subject.ThrowException($"Value is '{subject.Value.ToString()}' but should be IsError", because);
        return subject;
    }

    public static ShouldContext<Option<T>> BeNotFound<T>(this ShouldContext<Option<T>> subject, string? because = null)
    {
        if (subject.Value.IsNotFound() == false) subject.ThrowException($"Value is '{subject.Value.ToString()}' but should be IsNotFound", because);
        return subject;
    }

    public static ShouldContext<Option<T>> BeConflict<T>(this ShouldContext<Option<T>> subject, string? because = null)
    {
        if (subject.Value.IsConflict() == false) subject.ThrowException($"Value is '{subject.Value.ToString()}' but should be IsConflict", because);
        return subject;
    }
    public static ShouldContext<Option<T>> BeBadRequest<T>(this ShouldContext<Option<T>> subject, string? because = null)
    {
        if (subject.Value.IsBadRequest() == false) subject.ThrowException($"Value is '{subject.Value.ToString()}' but should be BeBadRequest", because);
        return subject;
    }
}
