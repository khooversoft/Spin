using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Toolbox.Telemetry;

public class TelemetryAggregator : ITelemetryCollector
{
    private Guid _instanceId = Guid.NewGuid();
    private ConcurrentQueue<TelemetryEvent> _events = new();
    private ConcurrentDictionary<string, List<TelemetryEvent>> _eventGroups = new(StringComparer.OrdinalIgnoreCase);
    private ConcurrentDictionary<string, long> _counterGroup = new(StringComparer.OrdinalIgnoreCase);
    private ConcurrentDictionary<string, long> _gaugeGroup = new(StringComparer.OrdinalIgnoreCase);

    public void Post(TelemetryEvent telemetryEvent)
    {
        _events.Enqueue(telemetryEvent);

        _eventGroups.AddOrUpdate(telemetryEvent.Name,
            _ => new List<TelemetryEvent> { telemetryEvent },

            (_, list) =>
            {
                list.Add(telemetryEvent);
                return list;
            }
        );

        switch(telemetryEvent.EventType)
        {
            case MetricDefinition.CounterType:
                UpdateCounter(telemetryEvent);
                break;

            case MetricDefinition.GaugeType:
                UpdateGauge(telemetryEvent);
                break;

            default:
                throw new InvalidOperationException($"Unsupported telemetry event type: {telemetryEvent.EventType}");
        }
    }

    public IReadOnlyList<TelemetryEvent> GetAllEvents() => _events.ToImmutableArray();

    public IReadOnlyList<TelemetryEvent> GetEventsByName(string name) => _eventGroups.TryGetValue(name, out var list)
        ? list.ToImmutableArray()
        : ImmutableArray<TelemetryEvent>.Empty;

    public void Clear()
    {
        _events.Clear();
        _eventGroups.Clear();
        _counterGroup.Clear();
    }

    public long GetCounterValue(string name) => _counterGroup.TryGetValue(name, out var value) ? value : -1;
    public long GetGaugeValue(string name) => _gaugeGroup.TryGetValue(name, out var value) ? value : -1;

    private void UpdateCounter(TelemetryEvent telemetryEvent)
    {
        if (telemetryEvent.TryGet<long>(out var longValue))
        {
            _counterGroup.AddOrUpdate(telemetryEvent.Name, _ => longValue, (_, existing) => existing + longValue);
        }
    }

    private void UpdateGauge(TelemetryEvent telemetryEvent)
    {
        if (telemetryEvent.TryGet<long>(out var longValue))
        {
            _gaugeGroup.AddOrUpdate(telemetryEvent.Name, _ => longValue, (_, existing) => longValue);
        }
    }
}
