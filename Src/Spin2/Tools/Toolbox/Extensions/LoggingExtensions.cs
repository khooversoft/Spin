using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Extensions;

public static class LoggingExtensions
{
    public static void Log(this ILogger logger, ScopeContextLocation context, LogLevel logLevel, string? message, params object?[] args)
    {
        context = context.With(logger);
        message = ConstructMessage(message);
        object[] newObjects = AddContext(args, context);

        logger.Log(logLevel, message, newObjects);
    }

    public static void LogInformation(this ILogger logger, ScopeContextLocation context, string? message, params object?[] args)
    {
        context = context.With(logger);
        message = ConstructMessage(message);
        object[] newObjects = AddContext(args, context);

        logger.LogInformation(message, newObjects);
    }

    public static void LogWarning(this ILogger logger, ScopeContextLocation context, string? message, params object?[] args)
    {
        context = context.With(logger);
        message = ConstructMessage(message);
        object[] newObjects = AddContext(args, context);

        logger.LogWarning(message, newObjects);
    }

    public static void LogError(this ILogger logger, ScopeContextLocation context, string? message, params object?[] args)
    {
        context = context.With(logger);
        message = ConstructMessage(message);
        object[] newObjects = AddContext(args, context);

        logger.LogError(message, newObjects);
    }

    public static void LogError(this ILogger logger, ScopeContextLocation context, Exception ex, string? message, params object?[] args)
    {
        context = context.With(logger);
        message = ConstructMessage(message);
        object[] newObjects = AddContext(args, context);

        logger.LogError(message, ex, newObjects);
    }

    public static void LogCritical(this ILogger logger, ScopeContextLocation context, string? message, params object?[] args)
    {
        context = context.With(logger);
        message = ConstructMessage(message);
        object[] newObjects = AddContext(args, context);

        logger.LogCritical(message, newObjects);
    }

    public static void LogCritical(this ILogger logger, ScopeContextLocation context, Exception ex, string? message, params object?[] args)
    {
        context = context.With(logger);
        message = ConstructMessage(message);
        object[] newObjects = AddContext(args, context);

        logger.LogCritical(message, ex, newObjects);
    }

    public static void LogTrace(this ILogger logger, ScopeContextLocation context, string? message, params object?[] args)
    {
        context = context.With(logger);
        message = ConstructMessage(message);
        object[] newObjects = AddContext(args, context);

        logger.LogTrace(message, newObjects);
    }

    public static IDisposable LogEntryExit(
        this ILogger logger,
        ScopeContext context,
        string? message = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0
    )
    {
        logger.NotNull().LogInformation(
            "Enter: Message={message}, Method={method}, path={path}, line={lineNumber}, traceId={traceId}",
            message ?? "<no message>",
            function,
            path,
            lineNumber,
            context.TraceId
            );

        var sw = Stopwatch.StartNew();

        return new FinalizeScope<ILogger>(logger, x => x.LogInformation(
                "Exit: Message={message}, ms={ms} Method={method}, path={path}, line={lineNumber}, traceId={traceId}",
                message ?? "<no message>",
                sw.ElapsedMilliseconds,
                function,
                path,
                lineNumber,
                context.TraceId
                )
            );
    }

    private static string ConstructMessage(string? message) => message switch
    {
        null => string.Empty,
        string v => v + ", ",
    } +
        "traceId={traceId}, " +
        "function={function}, " +
        "path={path}, " +
        "lineNumber={lineNumber}";

    private static object[] AddContext(object?[] args, ScopeContextLocation context) => (object[])(args ?? Array.Empty<object>())
        .OfType<object>()
        .Select(x => (x.GetType().IsClass) switch
        {
            true => x.ToJsonPascalSafe(context.Context),
            false => x
        })
        .Append(context.Context.TraceId)
        .Append(context.Location.Function)
        .Append(context.Location.Path)
        .Append(context.Location.LineNumber)
        .ToArray();
}
