using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools;

public static class LoggingOptionExtensions
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
        var location = context.Location(function: function, path: path, lineNumber: lineNumber);

        InternalLogStatus(location, option, context, message, args, name);
        return option;
    }
    
    public static Option LogStatus(
        this (StatusCode statusCode, string? error) subject,
        ScopeContext context,
        string message,
        IEnumerable<object>? args = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        )
    {
        var location = context.Location(function: function, path: path, lineNumber: lineNumber);

        var option = subject.ToOptionStatus();
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
        var location = context.Location(function: function, path: path, lineNumber: lineNumber);

        InternalLogStatus(location, option.ToOptionStatus(), context, message, args, name);
        return option;
    }

    public static string ToSafeLoggingFormat(this string subject) => (subject ?? string.Empty).Replace("{", "{{").Replace("}", "}}");

    private static void InternalLogStatus(
        ScopeContextLocation location,
        Option option,
        ScopeContext context,
        string message,
        IEnumerable<object>? args,
        string name,
        bool forceDebug = false
        )
    {
        var result = new StructureLineBuilder()
            .Add(message.NotEmpty(), args)
            .Add(option)
            .Add(context)
            .Add("argumentName={argumentName}", name)
            .Build();

        LogLevel logLevel = forceDebug || option.StatusCode.IsOk() ? LogLevel.Debug : LogLevel.Error;
        context.Context.Logger.Log(logLevel, result.Message, result.Args);
    }
}
