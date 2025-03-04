using System.Collections.Concurrent;
using Toolbox.Extensions;


namespace Toolbox.Tools;

public readonly struct MonitorRateStats
{
    public float Tps { get; init; }
    public bool IsOverThreshold { get; init; }
    public int QueueCount { get; init; }
}

public class MonitorRate
{
    private readonly ConcurrentQueue<DateTime> _queue = new ConcurrentQueue<DateTime>();
    private readonly TimeSpan _windowSize;
    private readonly float _tpsThreshold;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly object _lock = new object();
    private readonly float? _maxBrustTps;
    private readonly TimeSpan? _minDelaySpan;
    private DateTime? _lastDelayDate;
    private DateTime _lastCleaned = DateTime.MinValue;
    private static TimeSpan _cleanSpan = TimeSpan.FromMilliseconds(200);

    public MonitorRate(TimeSpan windowSize, float tpsThreshold, float? maxBrustTps = null)
    {
        _windowSize = windowSize.Assert(x => x.TotalSeconds >= 1, x => $"windowSize={x.TotalSeconds}sec is too low, > 1");
        _maxBrustTps = maxBrustTps;
        _minDelaySpan = maxBrustTps.HasValue ? TimeSpan.FromSeconds(1 / maxBrustTps.Value) : null;

        _tpsThreshold = tpsThreshold
            .Func(x => (float)Math.Truncate(x * 10) / 10)
            .Assert(x => x > 0.1, x => $"tps={x} is too low, min=0.1");
    }

    public MonitorRateStats Stats { get; private set; }

    public void RecordEvent()
    {
        _queue.Enqueue(DateTime.UtcNow);
        CalculateTPS();
    }

    public async Task RecordEventAsync(CancellationToken token = default)
    {
        RecordEvent();
        await WhenUnder(token);
    }

    public async Task WhenUnder(CancellationToken token = default)
    {
        DateTime now = DateTime.UtcNow;

        if (_minDelaySpan != null && (_lastDelayDate == null || now - _lastDelayDate > _minDelaySpan))
        {
            await Task.Delay(_minDelaySpan.Value, token);
            _lastDelayDate = now;
        }

        await _semaphore.WaitAsync(token);

        try
        {
            CalculateTPS();

            while (!token.IsCancellationRequested && Stats.IsOverThreshold)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                CalculateTPS();
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void CalculateTPS()
    {
        lock (_lock)
        {
            DateTime now = DateTime.UtcNow;
            if (now - _lastCleaned >= _cleanSpan)
            {
                _lastCleaned = now;
                DateTime windowStart = now - _windowSize;
                while (_queue.TryPeek(out DateTime peek) && peek < windowStart) { _queue.TryDequeue(out _); }
            }

            float tps = truncate(_queue.Count / (float)_windowSize.TotalSeconds);

            var result = new MonitorRateStats
            {
                QueueCount = _queue.Count,
                Tps = tps,
                IsOverThreshold = tps >= _tpsThreshold,
            };

            Stats = result;
        }

        float truncate(float value) => (float)Math.Truncate(value * 10000) / 10000;
    }
}
