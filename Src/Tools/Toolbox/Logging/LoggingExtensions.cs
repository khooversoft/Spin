using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Logging;

public static class LoggingExtensions
{
    public static Option LogStatus(
        this Option option,
        ScopeContext context,
        string message,
        IEnumerable<object>? args = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("option")] string name = ""
        )
    {
        message.NotEmpty();
        var location = context.Location(function: function, path: path, lineNumber: lineNumber);

        InternalLogStatus(location, option, context, message, args, name);
        return option;
    }

    public static Option<T> LogStatus<T>(
        this Option<T> option,
        ScopeContext context,
        string message,
        IEnumerable<object>? args = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("option")] string name = ""
        )
    {
        message.NotEmpty();
        var location = context.Location(function: function, path: path, lineNumber: lineNumber);

        InternalLogStatus(location, option.ToOptionStatus(), context, message, args, name);
        return option;
    }

    private static void InternalLogStatus(ScopeContextLocation location, Option option, ScopeContext context, string message, IEnumerable<object>? args, string name)
    {
        message.NotEmpty();

        object?[] argList = args?.ToArray() ?? Array.Empty<object>();

        string msg = ScopeContextTools.AppendMessage(message, "Argument={argumentName}, StatusCode={statusCode}, Error={error}");
        argList = ScopeContextTools.AppendArgs(argList, name, option.StatusCode, option.Error ?? "<no error>");
        (msg, argList) = context.AppendContext(msg, argList);

        location.Log(option.StatusCode.IsOk() ? LogLevel.Information : LogLevel.Error, msg, argList);
    }
}
