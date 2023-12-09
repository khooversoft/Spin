using Microsoft.ApplicationInsights;
using Toolbox.Metrics;

namespace Toolbox.Azure.Extensions;

public class AzureAppInsightsMetric : IMetric
{
    private readonly TelemetryClient _telemetryClient;

    public AzureAppInsightsMetric(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public void TrackValue(string name, double value) => _telemetryClient.GetMetric(name).TrackValue(value);
}
