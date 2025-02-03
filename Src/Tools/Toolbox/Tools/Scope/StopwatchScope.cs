using System.Diagnostics;
using Toolbox.Types;

namespace Toolbox.Tools;

public readonly struct StopwatchScope : IDisposable
{
    private readonly ScopeContext? _context = null;
    private readonly string? _metricName;
    private readonly string? _message = null;
    private readonly object?[] _args = [];

    public StopwatchScope() => Timestamp = Stopwatch.GetTimestamp();

    public StopwatchScope(ScopeContext context, string metricName, string? message = null, params object?[] args)
    {
        _context = context;
        _metricName = metricName.NotEmpty();
        _message = message;
        _args = args.NotNull();
        Timestamp = Stopwatch.GetTimestamp();
    }

    public long Timestamp { get; }
    public TimeSpan Elapsed => Stopwatch.GetElapsedTime(Timestamp);

    public TimeSpan Log(string? tag = null)
    {
        string name = _metricName.NotNull() + (tag == null ? string.Empty : "." + tag);
        _context?.LogMetric(name, "ms", Elapsed.TotalMilliseconds, _message, _args);
        return Elapsed;
    }

    public void Dispose() => Log();
}