using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Telemetry;

public interface ITelemetryCollector
{
    void Post(TelemetryEvent telemetryEvent);
}

public class TelemetryCollector : ITelemetryCollector
{
    private readonly ConcurrentDictionary<string, ITelemetryCollector> _collectors = new(StringComparer.OrdinalIgnoreCase);
    private readonly IServiceProvider _service;
    private readonly ILogger<TelemetryCollector> _logger;

    public TelemetryCollector(TelemetryOption option, IServiceProvider service, ILogger<TelemetryCollector> logger)
    {
        option.NotNull();
        _service = service.NotNull();
        _logger = logger.NotNull();

        _collectors = option.GetCollectors()
            .Select(x => x(service))
            .ToConcurrentDictionary(x => x.GetType().FullName ?? x.GetType().Name, StringComparer.OrdinalIgnoreCase);
    }

    public int Count => _collectors.Count;

    public bool AddCollector(ITelemetryCollector collector)
    {
        collector.NotNull();
        string name = collector.GetType().FullName ?? collector.GetType().Name;
        return _collectors.TryAdd(name, collector);
    }

    public bool AddCollector(string name, ITelemetryCollector collector)
    {
        name.NotEmpty();
        collector.NotNull();
        return _collectors.TryAdd(name, collector);
    }

    public void Post(TelemetryEvent telemetryEvent) => _collectors.Values.ForEach(x => x.Post(telemetryEvent));

    public IDisposable AddCollector<T>(string name, T instance) where T : ITelemetryCollector
    {
        name.NotEmpty();
        instance.NotNull();

        _collectors.TryAdd(name, instance).BeTrue("Instances already exists");

        var scope = new FinalizeScope(() => _collectors.TryRemove(name, out var _));
        return scope;
    }

    public bool TryRemoveCollector(string name) => _collectors.TryRemove(name, out var _);
}
