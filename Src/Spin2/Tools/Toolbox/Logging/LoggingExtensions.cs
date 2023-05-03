using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Types;

namespace Toolbox.Logging;

public static class LoggingExtensions
{
    public static void LogInformation(this ILogger logger, ScopeContextLocation context, string? message, params object?[] args)
    {
        message = ConstructMessage(context, message);
        object[] newObjects = AddContext(args, context);

        logger.LogInformation(message, newObjects);
    }

    public static void LogWarning(this ILogger logger, ScopeContextLocation context, string? message, params object?[] args)
    {
        message = ConstructMessage(context, message);
        object[] newObjects = AddContext(args, context);

        logger.LogWarning(message, newObjects);
    }

    public static void LogError(this ILogger logger, ScopeContextLocation context, string? message, params object?[] args)
    {
        message = ConstructMessage(context, message);
        object[] newObjects = AddContext(args, context);

        logger.LogError(message, newObjects);
    }

    public static void LogError(this ILogger logger, ScopeContextLocation context, Exception ex, string? message, params object?[] args)
    {
        message = ConstructMessage(context, message);
        object[] newObjects = AddContext(args, context);

        logger.LogError(message, ex, newObjects);
    }

    public static void LogCritical(this ILogger logger, ScopeContextLocation context, string? message, params object?[] args)
    {
        message = ConstructMessage(context, message);
        object[] newObjects = AddContext(args, context);

        logger.LogCritical(message, newObjects);
    }

    public static void LogCritical(this ILogger logger, ScopeContextLocation context, Exception ex, string? message, params object?[] args)
    {
        message = ConstructMessage(context, message);
        object[] newObjects = AddContext(args, context);

        logger.LogCritical(message, ex, newObjects);
    }

    public static void LogTrace(this ILogger logger, ScopeContextLocation context, string? message, params object?[] args)
    {
        message = ConstructMessage(context, message);
        object[] newObjects = AddContext(args, context);

        logger.LogTrace(message, newObjects);
    }

    private static string ConstructMessage(ScopeContextLocation context, string? message) => (message ?? string.Empty) +
        "traceId={traceId}, " +
        "function={function}, " +
        "path={path}, " +
        "lineNumber={lineNumber}";

    private static object[] AddContext(object?[] args, ScopeContextLocation context) => (object[])(args ?? Array.Empty<object>())
        .Append(context.Context.TraceId)
        .Append(context.Location.Function)
        .Append(context.Location.Path)
        .Append(context.Location.LineNumber)
        .ToArray();
}
