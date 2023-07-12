﻿using System.Net;
using Toolbox.Tools;
using Toolbox.Tools.Validation;

namespace Toolbox.Types;

public static class OptionExtensions
{
    public static T Return<T>(this Option<T> subject, bool throwOnNoValue = false) => subject.HasValue switch
    {
        true => subject.Value,
        false => throwOnNoValue ? throw new ArgumentException("HasValue is false") : default!,
    };

    public static async Task<T> Return<T>(this Task<Option<T>> subject, bool throwOnNoValue = false) => (await subject) switch
    {
        var v when v.HasValue => v.Value,
        _ => throwOnNoValue ? throw new ArgumentException("HasValue is false") : default!,
    };

    public static T Return<T>(this Option<T> subject, Func<T> defaultValue) => subject switch
    {
        var v when !v.HasValue => defaultValue(),
        var v => v.Return(),
    };

    public static Option Unwrap(this Option<Option> subject) => subject switch
    {
        var v when v.StatusCode.IsError() => new Option(v.StatusCode, v.Error),
        var v => v.Return(),
    };

    public static Option<T> Unwrap<T>(this Option<Option<T>> subject) => subject switch
    {
        var v when v.StatusCode.IsError() => new Option<T>(v.StatusCode, v.Error),
        var v => v.Return(),
    };

    public static async Task<Option> UnwrapAsync(this Task<Option<Option>> subject) => (await subject).Unwrap();
    public static async Task<Option<T>> UnwrapAsync<T>(this Task<Option<Option<T>>> subject) => (await subject).Unwrap();

    public static Option<T> ToOption<T>(this T? value) => new Option<T>(value);
    public static Option<T> ToOption<T>(this IOption subject) => new Option<T>(subject.StatusCode, subject.Error);

    public static Option LogResult(this Option subject, ScopeContextLocation location)
    {
        location.Context.Logger.NotNull();
        if (subject.StatusCode.IsOk()) return subject;

        location.LogError("Option = IsError, statusCode={statusCode}, error={error}", subject.StatusCode, subject.Error);
        return subject;
    }

    public static Option<T> LogResult<T>(this Option<T> subject, ScopeContextLocation location)
    {
        location.Context.Logger.NotNull();
        if (subject.StatusCode.IsOk()) return subject;

        location.LogError("Option = IsError, statusCode={statusCode}, error={error}", subject.StatusCode, subject.Error);
        return subject;
    }
}