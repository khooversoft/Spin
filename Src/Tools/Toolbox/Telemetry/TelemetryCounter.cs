namespace Toolbox.Telemetry;

public interface ITelemetryCounter<T> where T : struct
{
    void Increment(string? scope = null, string? tags = null);
    void Add(T value, string? scope = null, string? tags = null);
}


public class TelemetryCounter<T> : TelemetryBase<T>, ITelemetryCounter<T> where T : struct
{
    public TelemetryCounter(MetricDefinition metricDefinition, ITelemetryCollector telemetryCollector)
        : base(metricDefinition, telemetryCollector)
    {
    }

    public void Increment(string? scope = null, string? tags = null)
    {
        T incrementValue = typeof(T) switch
        {
            var type when type == typeof(int) => (T)(object)1,
            var type when type == typeof(long) => (T)(object)1L,
            _ => throw new NotSupportedException($"Increment is not supported for value type {typeof(T).Name}")
        };

        PostInternal(incrementValue, scope, tags);
    }

    public void Add(T value, string? scope = null, string? tags = null) => PostInternal(value, scope, tags);
}
