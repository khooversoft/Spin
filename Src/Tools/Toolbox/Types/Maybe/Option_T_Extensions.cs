using System.Diagnostics;

namespace Toolbox.Types;

public static class Option_T_Extensions
{
    public static bool IsOk<T>(this Option<T> subject) => subject.StatusCode.IsOk();
    public static bool IsNoContent<T>(this Option<T> subject) => subject.StatusCode.IsNoContent();
    public static bool IsSuccess<T>(this Option<T> subject) => subject.StatusCode.IsSuccess();
    public static bool IsError<T>(this Option<T> subject) => subject.StatusCode.IsError();
    public static bool IsNotFound<T>(this Option<T> subject) => subject.StatusCode.IsNotFound();
    public static bool IsBadRequest<T>(this Option<T> subject) => subject.StatusCode.IsBadRequest();

    [DebuggerStepThrough]
    public static T Return<T>(this Option<T> subject, bool throwOnNoValue = true, string? error = null) => subject.HasValue switch
    {
        true => subject.Value,
        false => throwOnNoValue ? throw new ArgumentException(error ?? "HasValue is false") : default!,
    };

    [DebuggerStepThrough]
    public static async Task<T> Return<T>(this Task<Option<T>> subject, bool throwOnNoValue = true, string? error = null) => (await subject) switch
    {
        var v when v.HasValue => v.Value,
        _ => throwOnNoValue ? throw new ArgumentException(error ?? "HasValue is false") : default!,
    };

    [DebuggerStepThrough]
    public static T Return<T>(this Option<T> subject, Func<T> defaultValue) => subject switch
    {
        var v when !v.HasValue => defaultValue(),
        var v => v.Return(),
    };

    [DebuggerStepThrough]
    public static Option<T> ToOption<T>(this T? value) => new Option<T>(value);

    [DebuggerStepThrough]
    public static Option<T> ToOptionStatus<T>(this IOption subject) => new Option<T>(subject.StatusCode, subject.Error);

    [DebuggerStepThrough]
    public static Option ToOptionStatus<T>(this Option<T> subject) => new Option(subject.StatusCode, subject.Error);

    [DebuggerStepThrough]
    public static Option ToOptionStatus(this bool subject, StatusCode falseStatusCode = StatusCode.BadRequest, string? error = null) =>
        new Option(subject ? StatusCode.OK : falseStatusCode, subject ? null : error);
}