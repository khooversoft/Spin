using System.Diagnostics;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Logging;

public static class Metric
{
    public static StopwatchScope LogDuration(this ScopeContext context, string metricName, string? message = null, params object?[] args)
    {
        long timestamp = Stopwatch.GetTimestamp();

        return new StopwatchScope(context, metricName, message, args);
    }
}
