using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Types;

public static class OptionExtensionTools
{
    public static Option<T> FirstOrDefaultOption<T>(this IEnumerable<T> source)
    {
        foreach (T element in source) return new Option<T>(element);
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

    public static Option ThrowOnError(this Option option) => option
        .Assert(x => x.StatusCode.IsOk(), x => $"Option is error, statusCode={x.StatusCode}");

    public static async Task<Option> ThrowOnError(this Task<Option> option) => (await option)
        .Assert(x => x.StatusCode.IsOk(), x => $"Option is error, statusCode={x.StatusCode}");

    public static Option<T> ThrowOnError<T>(this Option<T> option) => option
        .Assert(x => x.StatusCode.IsOk(), x => $"Option<T> (T={typeof(T).FullName}) is error, statusCode={x.StatusCode}");

    public static async Task<Option<T>> ThrowOnError<T>(this Task<Option<T>> option) => (await option)
        .Assert(x => x.StatusCode.IsOk(), x => $"Option<T> (T={typeof(T).FullName}) is error, statusCode={x.StatusCode}");
}
