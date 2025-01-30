using Microsoft.Extensions.Logging;

namespace Toolbox.Types;

public static class LoggerContextExtensions
{
    public static void Log(this ILoggingContext context, LogLevel logLevel, string? message, params object?[] args)
    {
        (string? newMessage, object?[] newObjects) = context.AppendContext(message, args);
        context.Context.Logger.Log(logLevel, newMessage, newObjects);
    }

    public static void LogInformation(this ILoggingContext context, string? message, params object?[] args)
    {
        (string? newMessage, object?[] newObjects) = context.AppendContext(message, args);
        context.Context.Logger.LogInformation(newMessage, newObjects);
    }

    public static void LogWarning(this ILoggingContext context, string? message, params object?[] args)
    {
        (string? newMessage, object?[] newObjects) = context.AppendContext(message, args);
        context.Context.Logger.LogWarning(newMessage, newObjects);
    }

    public static void LogWarning(this ILoggingContext context, Exception ex, string? message, params object?[] args)
    {
        (string? newMessage, object?[] newObjects) = context.AppendContext(message, args);
        context.Context.Logger.LogWarning(ex, newMessage, newObjects);
    }

    public static void LogError(this ILoggingContext context, string? message, params object?[] args)
    {
        (string? newMessage, object?[] newObjects) = context.AppendContext(message, args);
        context.Context.Logger.LogError(newMessage, newObjects);
    }

    public static void LogError(this ILoggingContext context, Exception ex, string? message, params object?[] args)
    {
        (string? newMessage, object?[] newObjects) = context.AppendContext(message, args);
        context.Context.Logger.LogError(ex, newMessage, newObjects);
    }

    public static void LogCritical(this ILoggingContext context, string? message, params object?[] args)
    {
        (string? newMessage, object?[] newObjects) = context.AppendContext(message, args);
        context.Context.Logger.LogCritical(newMessage, newObjects);
    }

    public static void LogCritical(this ILoggingContext context, Exception ex, string? message, params object?[] args)
    {
        (string? newMessage, object?[] newObjects) = context.AppendContext(message, args);
        context.Context.Logger.LogCritical(ex, newMessage, newObjects);
    }

    public static void LogTrace(this ILoggingContext context, string? message, params object?[] args)
    {
        (string? newMessage, object?[] newObjects) = context.AppendContext(message, args);
        context.Context.Logger.LogTrace(newMessage, newObjects);
    }

    public static void LogMetric(this ILoggingContext context, string metricName, string unit, double value, string? message = null, params object?[] args)
    {
        var metricMessage = "[metric:{metricName} value={value}, unit={unit}]" + (message == null ? string.Empty : " " + message);
        var metricArgs = new object?[] { metricName, value, unit }.Concat(args).ToArray();

        (string? newMessage, object?[] newObjects) = context.AppendContext(metricMessage, metricArgs);
        context.Context.Logger.LogInformation(newMessage, newObjects);
    }
}