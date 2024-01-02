using Microsoft.Extensions.Logging;

namespace Toolbox.Types;

public interface ILoggingContext
{
    public ScopeContext Context { get; }
    public (string? message, object?[] args) AppendContext(string? message, object?[] args);
}


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


}