using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Toolbox.Metrics;
using Toolbox.Tools;

namespace Toolbox.Azure.Extensions;

public static class LoggerExtensions
{
    public static IDisposable TrackPerformance(this TelemetryClient telemetryClient, string name)
    {
        telemetryClient.NotNull();
        name.NotEmpty();

        var sw = Stopwatch.StartNew();

        return new FinalizeScope<TelemetryClient>(telemetryClient, x =>
        {
            sw.Stop();
            telemetryClient.GetMetric(name).TrackValue(sw.ElapsedMilliseconds);
        });
    }


}


public class MetricClient : IMetric
{
    public void TrackValue(string name, double value)
    {
        throw new NotImplementedException();
    }
}