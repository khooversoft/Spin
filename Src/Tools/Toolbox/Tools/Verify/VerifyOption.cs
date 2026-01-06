using System.Diagnostics;
using System.Runtime.CompilerServices;
using Toolbox.Types;

namespace Toolbox.Tools;

public static class VerifyOption
{
    [DebuggerStepThrough]
    public static Option Be(this Option subject, StatusCode value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject.StatusCode != value) throw new ArgumentException(Verify.FormatException($"'{name}' is '{subject.StatusCode}' should be '{value}', error={subject.Error}", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static Option NotBe(this Option subject, StatusCode value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject.StatusCode == value) throw new ArgumentException(Verify.FormatException($"'{name}' is '{subject.StatusCode}' should NOT be '{value}', error={subject.Error}", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static Option<T> Be<T>(this Option<T> subject, StatusCode value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject.StatusCode != value) throw new ArgumentException(Verify.FormatException($"'{name}' is '{subject.StatusCode}' should be '{value}', error={subject.Error}", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static Option<T> NotBe<T>(this Option<T> subject, StatusCode value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject.StatusCode == value) throw new ArgumentException(Verify.FormatException($"'{name}' is '{subject.StatusCode}' should NOT be '{value}', error={subject.Error}", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static Option BeOk(this Option subject, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (!subject.StatusCode.IsOk()) throw new ArgumentException(Verify.FormatException($"'{name}' is '{subject.StatusCode}' should be OK, error={subject.Error}", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static Option<T> BeOk<T>(this Option<T> subject, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (!subject.StatusCode.IsOk()) throw new ArgumentException(Verify.FormatException($"'{name}' is '{subject.StatusCode}' should be OK, error={subject.Error}", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static Option BeError(this Option subject, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (!subject.StatusCode.IsError()) throw new ArgumentException(Verify.FormatException($"'{name}' is '{subject.StatusCode}' should be Error, error={subject.Error}", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static Option<T> BeError<T>(this Option<T> subject, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (!subject.StatusCode.IsError()) throw new ArgumentException(Verify.FormatException($"'{name}' is '{subject.StatusCode}' should be Error, error={subject.Error}", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static Option BeNotFound(this Option subject, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (!subject.StatusCode.IsNotFound()) throw new ArgumentException(Verify.FormatException($"'{name}' is '{subject.StatusCode}' should be NotFound, error={subject.Error}", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static Option<T> BeNotFound<T>(this Option<T> subject, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (!subject.StatusCode.IsNotFound()) throw new ArgumentException(Verify.FormatException($"'{name}' is '{subject.StatusCode}' should be NotFound, error={subject.Error}", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static Option BeConflict(this Option subject, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (!subject.StatusCode.IsConflict()) throw new ArgumentException(Verify.FormatException($"'{name}' is '{subject.StatusCode}' should be Conflict, error={subject.Error}", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static Option<T> BeConflict<T>(this Option<T> subject, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (!subject.StatusCode.IsConflict()) throw new ArgumentException(Verify.FormatException($"'{name}' is '{subject.StatusCode}' should be Conflict, error={subject.Error}", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static Option BeBadRequest(this Option subject, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (!subject.StatusCode.IsBadRequest()) throw new ArgumentException(Verify.FormatException($"'{name}' is '{subject.StatusCode}' should be BadRequest, error={subject.Error}", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static Option<T> BeBadRequest<T>(this Option<T> subject, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (!subject.StatusCode.IsBadRequest()) throw new ArgumentException(Verify.FormatException($"'{name}' is '{subject.StatusCode}' should be BadRequest, error={subject.Error}", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static Option BeLocked(this Option subject, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (!subject.StatusCode.IsLocked()) throw new ArgumentException(Verify.FormatException($"'{name}' is '{subject.StatusCode}' should be Locked, error={subject.Error}", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static Option<T> BeLocked<T>(this Option<T> subject, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (!subject.StatusCode.IsLocked()) throw new ArgumentException(Verify.FormatException($"'{name}' is '{subject.StatusCode}' should be Locked, error={subject.Error}", because));
        return subject;
    }
}
