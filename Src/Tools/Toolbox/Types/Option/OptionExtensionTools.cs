using System.Diagnostics;
using System.Runtime.CompilerServices;
using Toolbox.Tools;

namespace Toolbox.Types;

public static class OptionExtensionTools
{
    [DebuggerStepThrough]
    public static Option ThrowOnError(this Option option, string? message = "< no message >", [CallerArgumentExpression("option")] string name = "")
    {
        return option.Assert(x => x.StatusCode.IsOk(), x => $"message={message}, statusCode={x.StatusCode}, error={x.Error}", name: name);
    }

    [DebuggerStepThrough]
    public static async Task<Option> ThrowOnError(this Task<Option> option, string? message = "< no message >", [CallerArgumentExpression("option")] string name = "")
    {
        return (await option).Assert(x => x.StatusCode.IsOk(), x => $"message={message}, statusCode={x.StatusCode}, error={x.Error}", name: name);
    }

    [DebuggerStepThrough]
    public static Option<T> ThrowOnError<T>(this Option<T> option, string? message = "< no message >", [CallerArgumentExpression("option")] string name = "")
    {
        return option.Assert(x => x.StatusCode.IsOk(), x => $"message={message}, statusCode={x.StatusCode}, error={x.Error}");
    }

    [DebuggerStepThrough]
    public static async Task<Option<T>> ThrowOnError<T>(this Task<Option<T>> option, string? message = "< no message >", [CallerArgumentExpression("option")] string name = "")
    {
        return (await option).Assert(x => x.StatusCode.IsOk(), x => $"message={message}, statusCode={x.StatusCode}, error={x.Error}", name: name);
    }
}
