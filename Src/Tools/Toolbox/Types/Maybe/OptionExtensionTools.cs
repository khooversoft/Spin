using System.Runtime.CompilerServices;
using Toolbox.Tools;

namespace Toolbox.Types;

public static class OptionExtensionTools
{
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

    public static Option LogOnError(this Option option,
        ScopeContext context,
        string? message = "< no message >",
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("option")] string name = ""
        )
    {
        if (option.IsError())
        {
            string msg = $"Message={message}, StatusCode={option.StatusCode}, Error={option.Error}";
            context.Location(function: function, path: path, lineNumber: lineNumber).LogError(msg);
        }

        return option;
    }

    public static Option<T> LogOnError<T>(this Option<T> option,
        ScopeContext context,
        string? message = "< no message >",
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("option")] string name = ""
        )
    {
        if (option.IsError())
        {
            string msg = $"Message={message}, StatusCode={option.StatusCode}, Error={option.Error}";
            context.Location(function: function, path: path, lineNumber: lineNumber).LogError(msg);
        }

        return option;
    }
}
