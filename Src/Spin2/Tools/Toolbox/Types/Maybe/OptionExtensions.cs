using System;

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
    public static Option<T> ToOption<T>(this T? value, OptionStatus statusCode) => new Option<T>(value, statusCode);
    public static Option<T> ToOption<T>(this OptionStatus statusCode) => new Option<T>(statusCode);

    public static Option<T> ToOption<T>(this (bool hasValue, OptionStatus statusCode, T value) value) => new Option<T>(value.hasValue, value.statusCode, value.value);
    public static Option<T> ToOption<T>(this (bool hasValue, T value) value) => new Option<T>(value.hasValue, value.value);

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
}