using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Types;
using Toolbox.Tools;

namespace Toolbox.Logging;

public static class Metric
{
    public static StopwatchScope LogDuration(this ScopeContext context, string metricName, string? message = null, params object?[] args)
    {
        long timestamp = Stopwatch.GetTimestamp();

        return new StopwatchScope(context, metricName, message, args);
    }
}
