using Toolbox.Extensions;

namespace Toolbox.Types;

public interface ILoggingFormatter
{
    string ConstructMessage(string? message);
    object[] AddContext(object?[] args, ScopeContextLocation context);
}

public static class LoggingFormatter
{
    public static ILoggingFormatter LoggingFormatterLocation { get; } = new LoggingFormatterLocation();
    public static ILoggingFormatter LoggingTrace { get; } = new LoggingTrace();
}

public class LoggingFormatterLocation : ILoggingFormatter
{
    public string ConstructMessage(string? message) => message switch
    {
        null => string.Empty,
        string v => v + ", ",
    } +
        "traceId={traceId}, " +
        "callerFunction={callerFunction}, " +
        "callerFilePath={callerFilePath}, " +
        "callerLineNumber={callerLineNumber}";

    public object[] AddContext(object?[] args, ScopeContextLocation context) => (object[])(args ?? Array.Empty<object?>())
        .Select(x => (x?.GetType().IsClass == true) switch
        {
            true => x.ToJsonPascalSafe(context.Context),
            false => x
        })
        .Append(context.Context.TraceId)
        .Append(context.Location.CallerFunction)
        .Append(context.Location.CallerFilePath)
        .Append(context.Location.CallerLineNumber)
        .ToArray();
}

public class LoggingTrace : ILoggingFormatter
{
    public string ConstructMessage(string? message) => message switch
    {
        null => string.Empty,
        string v => v + ", ",
    } +
        "traceId={traceId}";

    public object[] AddContext(object?[] args, ScopeContextLocation context) => (object[])(args ?? Array.Empty<object?>())
        .Select(x => (x?.GetType().IsClass == true) switch
        {
            true => x.ToJsonPascalSafe(context.Context),
            false => x
        })
        .Append(context.Context.TraceId)
        .ToArray();
}
