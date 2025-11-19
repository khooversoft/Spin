using System;
using System.Threading;
using System.Threading.Tasks;

namespace Toolbox.Tools;

public sealed class RateLimiter
{
    private readonly object _gate = new();
    private readonly TimeProvider _timeProvider;

    // Configuration
    private readonly double _tokensPerSecond;   // Refill rate
    private readonly double _maxTokens;         // Burst capacity

    // State
    private double _availableTokens;
    private long _lastRefillTimestamp;

    // Accumulates fractional refill so we only add whole tokens, improving determinism for tests
    private double _refillRemainder;

    /// <summary>
    /// Create a rate limiter that allows <paramref name="units"/> in each <paramref name="per"/> interval.
    /// Examples:
    /// - TPS: new RateLimiter(10, TimeSpan.FromSeconds(1)) for 10 transactions per second.
    /// - Per minute: new RateLimiter(300, TimeSpan.FromMinutes(1)).
    /// Optional burst capacity defaults to the same number of units for the given interval.
    /// </summary>
    public RateLimiter(double units, TimeSpan per, double? burstCapacity = null, TimeProvider? timeProvider = null)
    {
        if (units <= 0) throw new ArgumentOutOfRangeException(nameof(units), "Units must be > 0.");
        if (per <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(per), "Interval must be > 0.");

        _timeProvider = timeProvider ?? TimeProvider.System;

        _tokensPerSecond = units / per.TotalSeconds;
        _maxTokens = burstCapacity is > 0 ? burstCapacity.Value : units;

        _availableTokens = _maxTokens;
        _lastRefillTimestamp = _timeProvider.GetTimestamp();

        Units = units;
        Interval = per;
        BurstCapacity = _maxTokens;
    }

    /// <summary>
    /// Number of units allowed per interval.
    /// </summary>
    public double Units { get; }

    /// <summary>
    /// The interval the <see cref="Units"/> value applies to.
    /// </summary>
    public TimeSpan Interval { get; }

    /// <summary>
    /// Maximum burst capacity in tokens (units).
    /// </summary>
    public double BurstCapacity { get; }

    /// <summary>
    /// Asynchronously waits until the requested number of permits can be acquired under the configured rate.
    /// If the target rate is exceeded, this method delays just enough to comply with the rate.
    /// </summary>
    /// <param name="permits">Number of units to consume.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async ValueTask WaitAsync(double permits = 1, CancellationToken cancellationToken = default)
    {
        if (permits <= 0) throw new ArgumentOutOfRangeException(nameof(permits), "Permits must be > 0.");
        cancellationToken.ThrowIfCancellationRequested();

        double waitSeconds;

        lock (_gate)
        {
            RefillUnlocked();

            // Reserve the permits now; negative balance means debt to be paid with time.
            _availableTokens -= permits;

            if (_availableTokens >= 0)
            {
                // Enough tokens, no wait required.
                return;
            }

            // Compute delay required to "earn back" the negative balance.
            waitSeconds = -_availableTokens / _tokensPerSecond;
        }

        // Perform the delay outside the lock.
        if (waitSeconds > 0)
        {
            var delay = TimeSpan.FromSeconds(waitSeconds);
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Attempts to acquire the requested permits immediately without waiting.
    /// Returns true if the permits were granted, otherwise false.
    /// </summary>
    public bool TryAcquire(double permits = 1)
    {
        if (permits <= 0) throw new ArgumentOutOfRangeException(nameof(permits), "Permits must be > 0.");

        lock (_gate)
        {
            RefillUnlocked();

            if (_availableTokens >= permits)
            {
                _availableTokens -= permits;
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Estimates how long you would need to wait right now in order to acquire the requested permits.
    /// </summary>
    public TimeSpan GetEstimatedDelay(double permits = 1)
    {
        if (permits <= 0) throw new ArgumentOutOfRangeException(nameof(permits), "Permits must be > 0.");

        lock (_gate)
        {
            RefillUnlocked();

            var projected = _availableTokens - permits;
            if (projected >= 0) return TimeSpan.Zero;

            var seconds = -projected / _tokensPerSecond;
            return TimeSpan.FromSeconds(seconds);
        }
    }

    /// <summary>
    /// Current available permits (reported as whole tokens, clamped to 0).
    /// Reporting whole tokens avoids sub-1 token noise between rapid operations.
    /// </summary>
    public double AvailablePermits
    {
        get
        {
            lock (_gate)
            {
                RefillUnlocked();
                return Math.Max(0, Math.Floor(_availableTokens));
            }
        }
    }

    private void RefillUnlocked()
    {
        var now = _timeProvider.GetTimestamp();
        var elapsed = _timeProvider.GetElapsedTime(_lastRefillTimestamp, now).TotalSeconds;

        if (elapsed <= 0) return;

        _lastRefillTimestamp = now;

        // Only add whole tokens, carry fractional remainder forward.
        var tokens = elapsed * _tokensPerSecond + _refillRemainder;
        var wholeTokens = Math.Floor(tokens);
        _refillRemainder = tokens - wholeTokens;

        if (wholeTokens <= 0) return;

        _availableTokens = Math.Min(_maxTokens, _availableTokens + wholeTokens);
    }
}
