using System.Runtime.CompilerServices;
using Toolbox.Tools;

namespace Toolbox.Types;

public static class OptionExtensionTools
{
    public static Option<T> FirstOrDefaultOption<T>(this IEnumerable<T> source)
    {
        foreach (T element in source) return element;
        return Option<T>.None;
    }

    public static Option<T> FirstOrDefaultOption<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        foreach (T element in source)
        {
            if (predicate(element)) return element;
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

    public static Option ThrowOnError(this Option option,
        string? message = "< no message >",
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("option")] string name = ""
        ) => option
        .Assert(
            x => x.StatusCode.IsOk(),
            x => $"Message={message}, StatusCode={x.StatusCode}, Error={x.Error}", function: function, path: path, lineNumber: lineNumber, name: name
            );

    public static async Task<Option> ThrowOnError(this Task<Option> option,
        string? message = "< no message >",
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("option")] string name = ""
        ) => (await option)
        .Assert(
            x => x.StatusCode.IsOk(),
            x => $"Message={message}, StatusCode={x.StatusCode}, Error={x.Error}", function: function, path: path, lineNumber: lineNumber, name: name
            );

    public static Option<T> ThrowOnError<T>(this Option<T> option,
        string? message = "< no message >",
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("option")] string name = ""
        ) => option
        .Assert(
            x => x.StatusCode.IsOk(),
            x => $"Message={message}, StatusCode={x.StatusCode}, Error={x.Error}", function: function, path: path, lineNumber: lineNumber, name: name
            );

    public static async Task<Option<T>> ThrowOnError<T>(this Task<Option<T>> option,
        string? message = "< no message >",
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("option")] string name = ""
        ) => (await option)
        .Assert(
            x => x.StatusCode.IsOk(),
            x => $"Message={message}, StatusCode= {x.StatusCode}, Error={x.Error}", function: function, path: path, lineNumber: lineNumber, name: name
            );
}
