using Microsoft.Extensions.DependencyInjection;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Telemetry;

public interface ITelemetry
{
    ITelemetryCounter<T> CreateCounter<T>(string name, string? description = null, string? unit = null, string? tags = null, string? version = null)
        where T : struct;
    ITelemetryRecorder<T> CreateHistogram<T>(string name, string? description = null, string? unit = null, string? tags = null, string? version = null)
        where T : struct;
    ITelemetryRecorder<T> CreateGauge<T>(string name, string? description = null, string? unit = null, string? tags = null, string? version = null)
        where T : struct;
}

public class TelemetryFactory : ITelemetry
{
    private readonly TelemetryCollector _collector;
    private readonly IServiceProvider _serviceProvider;

    public TelemetryFactory(TelemetryCollector collector, IServiceProvider serviceProvider)
    {
        _collector = collector.NotNull();
        _serviceProvider = serviceProvider.NotNull();
    }

    public ITelemetryCounter<T> CreateCounter<T>(string name, string? description = null, string? unit = null, string? tags = null, string? version = null)
        where T : struct
    {
        var def = CreateDefinition(name, MetricDefinition.CounterType, description, unit, tags, version);
        return ActivatorUtilities.CreateInstance<TelemetryCounter<T>>(_serviceProvider, def, _collector);
    }

    public ITelemetryRecorder<T> CreateHistogram<T>(string name, string? description = null, string? unit = null, string? tags = null, string? version = null)
        where T : struct
    {
        var def = CreateDefinition(name, MetricDefinition.HistogramType, description, unit, tags, version);
        return ActivatorUtilities.CreateInstance<TelemetryRecorder<T>>(_serviceProvider, def, _collector);
    }

    public ITelemetryRecorder<T> CreateGauge<T>(string name, string? description = null, string? unit = null, string? tags = null, string? version = null)
        where T : struct
    {
        var def = CreateDefinition(name, MetricDefinition.GaugeType, description, unit, tags, version);
        return ActivatorUtilities.CreateInstance<TelemetryRecorder<T>>(_serviceProvider, def, _collector);
    }

    private static MetricDefinition CreateDefinition(string name, string type, string? description, string? unit, string? tags, string? version)
    {

        name.NotEmpty();

        var definition = new MetricDefinition
        {
            Name = name,
            Type = type,
            Tags = tags,
            Version = version,
            Description = description,
            Unit = unit,
        };

        definition.Validate().ThrowOnError();
        return definition;
    }
}