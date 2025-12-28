namespace Toolbox.Telemetry;

public interface ITelemetryRecorder<T>
{
    void Post(T value, string? scope = null, string? tags = null);
}


public class TelemetryRecorder<T> : TelemetryBase<T>, ITelemetryRecorder<T> where T : struct
{
    public TelemetryRecorder(MetricDefinition metricDefinition, ITelemetryCollector telemetryCollector)
        : base(metricDefinition, telemetryCollector)
    {
    }

    public void Post(T value, string? scope = null, string? tags = null) => PostInternal(value, scope, tags);
}
