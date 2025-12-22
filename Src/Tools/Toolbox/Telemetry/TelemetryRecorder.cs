using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Telemetry;

public interface ITelemetryRecorder<T>
{
    void Post(string name, T value, string? scope = null, string? tags = null);
}


public class TelemetryRecorder<T> : ITelemetryRecorder<T>
{
    private readonly ITelemetryCollector _telemetryCollector;
    private readonly MetricDefinition _metricDefinition;

    public TelemetryRecorder(MetricDefinition metricDefinition, ITelemetryCollector telemetryCollector)
    {
        _metricDefinition = metricDefinition.NotNull();
        _telemetryCollector = telemetryCollector.NotNull();
    }

    public void Post(string name, T value, string? scope = null, string? tags = null)
    {
        name.NotEmpty();

        var tm = new TelemetryEvent
        {
            Name = name,
            Scope = scope == null ? _metricDefinition.Name : $"{_metricDefinition.Name}.{scope}",
            EventType = _metricDefinition.Type,
            Description = _metricDefinition.Description,
            Version = _metricDefinition.Version,
            ValueType = typeof(T).Name,
            Value = value?.ToString() ?? "null",
            Tags = _metricDefinition.Tags != null ? _metricDefinition.Tags + (tags != null ? $",{tags}" : string.Empty) : tags,
            Units = _metricDefinition.Unit,
        };

        tm.Validate().ThrowOnError();

        _telemetryCollector.Post(tm);
    }
}
