using System.Diagnostics;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Metrics;

public static class MetrixExtensions
{
    public static IDisposable TrackPerformance(this ScopeContext context, string name)
    {
        name.NotEmpty();

        var sw = Stopwatch.StartNew();
        return new FinalizeScope<IMetric>(context.Metric, x =>
        {
            sw.Stop();
            x.TrackValue(name, sw.ElapsedMilliseconds);
        });
    }
}
