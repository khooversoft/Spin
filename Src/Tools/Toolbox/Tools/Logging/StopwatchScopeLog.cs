using System.Diagnostics;
using Toolbox.Types;

namespace Toolbox.Tools;

public static class StopwatchScopeTool
{
    public static StopwatchScopeLog LogDuration(this ScopeContext context, string metricName, string? message = null, params object?[] args)
    {
        return new StopwatchScopeLog(context, metricName, message, args);
    }

    public static StopwatchScope LogDuration(this ScopeContext context, Action<string?, TimeSpan> action)
    {
        return new StopwatchScope(context, action);
    }
}

public readonly struct StopwatchScope : IDisposable
{
    private readonly ScopeContext? _context = null;
    private readonly Action<string?, TimeSpan> _action;

    public StopwatchScope(ScopeContext context, Action<string?, TimeSpan> action)
    {
        _context = context.NotNull();
        _action = action.NotNull();
        Timestamp = Stopwatch.GetTimestamp();
    }

    public long Timestamp { get; }
    public TimeSpan Elapsed => Stopwatch.GetElapsedTime(Timestamp);
    public void Dispose() => Log("Dispose");

    public TimeSpan Log(string? tag = null)
    {
        TimeSpan current = Elapsed;
        _action(tag, current);
        return current;
    }
}


public readonly struct StopwatchScopeLog : IDisposable
{
    private readonly ScopeContext? _context = null;
    private readonly string? _metricName;
    private readonly string? _message = null;
    private readonly object?[] _args = [];

    public StopwatchScopeLog(ScopeContext context, string metricName, string? message = null, params object?[] args)
    {
        _context = context;
        _metricName = metricName.NotEmpty();
        _message = message;
        _args = args.NotNull();
        Timestamp = Stopwatch.GetTimestamp();
    }

    public long Timestamp { get; }
    public TimeSpan Elapsed => Stopwatch.GetElapsedTime(Timestamp);
    public void Dispose() => Log("Dispose");

    public TimeSpan Log(string? tag = null)
    {
        string name = _metricName.NotNull() + (tag == null ? string.Empty : "." + tag);
        _context?.LogMetric(name, "ms", Elapsed.TotalMilliseconds, _message, _args);
        return Elapsed;
    }
}
