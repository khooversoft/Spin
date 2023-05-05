﻿using System;

namespace Toolbox.Types.Maybe;

public static class OptionTExtensions
{
    public static T Return<T>(this Option<T> subject) => subject.HasValue switch
    {
        true => subject.Value,
        false => default!,
    };

    public static T Return<T>(this Option<T> subject, Func<T> none) => subject switch
    {
        var v when !v.HasValue => none(),
        var v => v.Return(),
    };

    public static Option<T> ToOption<T>(this T? value) => new Option<T>(value);
    public static Option<T> ToOption<T>(this T? value, StatusCode statusCode) => new Option<T>(value, statusCode);
    public static Option<T> ToOption<T>(this StatusCode statusCode) => new Option<T>(statusCode);
    public static Option<T> ToOption<T>(this IOption subject) => new Option<T>(subject.StatusCode);

    public static Option<T> FirstOrDefaultOption<T>(this IEnumerable<T> source)
    {
        foreach (T element in source)
        {
            return new Option<T>(true, element);
        }

        return Option<T>.None;
    }

    public static Option<T> LastOrDefaultOption<T>(this IEnumerable<T> source)
    {
        using (IEnumerator<T> e = source.GetEnumerator())
        {
            if (e.MoveNext())
            {
                T result;
                do
                {
                    result = e.Current;
                }
                while (e.MoveNext());

                return new Option<T>(true, result);
            }
        }

        return Option<T>.None;
    }

    public static bool IsOk<T>(this Option<T> subject) => subject.StatusCode.IsOk();
    public static bool IsNoContent<T>(this Option<T> subject) => subject.StatusCode.IsNoContent();
    public static bool IsSuccess<T>(this Option<T> subject) => subject.StatusCode.IsSuccess();
    public static bool IsError<T>(this Option<T> subject) => subject.StatusCode.IsError();
    public static bool IsNotFound<T>(this Option<T> subject) => subject.StatusCode.IsNotFound();
}