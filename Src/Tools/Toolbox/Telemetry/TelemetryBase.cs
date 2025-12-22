using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Telemetry;

public class TelemetryBase<T>
{
    private readonly MetricDefinition _metricDefinition;
    private readonly ITelemetryCollector _telemetryCollector;

    public TelemetryBase(MetricDefinition metricDefinition, ITelemetryCollector telemetryCollector)
    {
        _metricDefinition = metricDefinition.NotNull();
        _telemetryCollector = telemetryCollector.NotNull();
    }

    protected void PostInternal(T value, string? scope = null, string? tags = null)
    {
        var tm = new TelemetryEvent
        {
            Name = _metricDefinition.Name,
            Scope = scope,
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

