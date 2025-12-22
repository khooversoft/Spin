using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;

namespace Toolbox.Telemetry;

public class TelemetryOption
{
    private ConcurrentQueue<Func<IServiceProvider, ITelemetryCollector>> _collectors { get; init; } = new();

    public IReadOnlyList<Func<IServiceProvider, ITelemetryCollector>> GetCollectors() => _collectors.ToImmutableArray();

    public TelemetryOption AddCollector<T>() where T : class, ITelemetryCollector
    {
        _collectors.Enqueue(serviceProvider => serviceProvider.GetRequiredService<T>());
        return this;
    }

    public TelemetryOption AddCollector<T>(T instance) where T : ITelemetryCollector
    {
        _collectors.Enqueue(_ => instance);
        return this;
    }

    public TelemetryOption AddCollector<T>(Func<IServiceProvider, T> factory) where T : ITelemetryCollector
    {
        _collectors.Enqueue(serviceProvider => factory(serviceProvider));
        return this;
    }
}
