using System.Diagnostics;

namespace Toolbox.Types;

[DebuggerStepThrough]
public static class Option_T_Extensions
{
    public static bool IsOk<T>(this Option<T> subject) => subject.StatusCode.IsOk();
    public static bool IsNoContent<T>(this Option<T> subject) => subject.StatusCode.IsNoContent();
    public static bool IsConflict<T>(this Option<T> subject) => subject.StatusCode.IsConflict();
    public static bool IsSuccess<T>(this Option<T> subject) => subject.StatusCode.IsSuccess();
    public static bool IsError<T>(this Option<T> subject) => subject.StatusCode.IsError();
    public static bool IsNotFound<T>(this Option<T> subject) => subject.StatusCode.IsNotFound();
    public static bool IsBadRequest<T>(this Option<T> subject) => subject.StatusCode.IsBadRequest();
    public static bool IsUnauthorized<T>(this Option<T> subject) => subject.StatusCode.IsUnauthorized();
    public static bool IsForbidden<T>(this Option<T> subject) => subject.StatusCode.IsForbidden();
    public static bool IsLocked<T>(this Option<T> subject) => subject.StatusCode.IsLocked();

    public static bool IsError<T>(this Option<T> subject, out Option result)
    {
        result = subject.ToOptionStatus();
        return subject.IsError();
    }

    public static Option<T> OutValue<T>(this Option<T> subject, out T value)
    {
        value = subject.HasValue ? subject.Value : default!;
        return subject;
    }

    public static T Return<T>(this Option<T> subject, bool throwOnNoValue = true, string? error = null) => subject.HasValue switch
    {
        true => subject.Value,
        false => throwOnNoValue ? throw new ArgumentException(error ?? "HasValue is false") : default!,
    };

    public static async Task<T> Return<T>(this Task<Option<T>> subject, bool throwOnNoValue = true, string? error = null) => (await subject) switch
    {
        var v when v.HasValue => v.Value,
        _ => throwOnNoValue ? throw new ArgumentException(error ?? "HasValue is false") : default!,
    };

    public static T Return<T>(this Option<T> subject, Func<T> defaultValue) => subject switch
    {
        var v when !v.HasValue => defaultValue(),
        var v => v.Return(),
    };

    public static Option<T> ToOption<T>(this T? value) => new Option<T>(value);

    public static Option<T> ToOptionStatus<T>(this IOption subject) => new Option<T>(subject.StatusCode, subject.Error);

    public static Option ToOptionStatus<T>(this Option<T> subject) => new Option(subject.StatusCode, subject.Error);

    public static Option ToOptionStatus(this bool subject, StatusCode falseStatusCode = StatusCode.BadRequest, string? error = null) =>
        new Option(subject ? StatusCode.OK : falseStatusCode, subject ? null : error);
}