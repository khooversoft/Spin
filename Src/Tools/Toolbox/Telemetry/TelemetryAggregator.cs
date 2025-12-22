using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Toolbox.Telemetry;

public class TelemetryAggregator : ITelemetryCollector
{
    private Guid _instanceId = Guid.NewGuid();
    private ConcurrentQueue<TelemetryEvent> _events = new();
    private ConcurrentDictionary<string, List<TelemetryEvent>> _eventGroups = new(StringComparer.OrdinalIgnoreCase);
    private ConcurrentDictionary<string, long> _longGroup = new(StringComparer.OrdinalIgnoreCase);

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

        if (telemetryEvent.TryGet<long>(out var longValue))
        {
            _longGroup.AddOrUpdate(telemetryEvent.Name, _ => longValue, (_, existing) => existing + longValue);
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
        _longGroup.Clear();
    }

    public long GetCounterValue(string name) => _longGroup.TryGetValue(name, out var value) ? value : -1;
}
