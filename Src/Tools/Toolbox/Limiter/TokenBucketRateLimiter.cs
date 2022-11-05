using System;
using System.Threading;
using Toolbox.Tools;

namespace Toolbox.Limiter;

public sealed class TokenBucketRateLimiter
{
    private readonly int _tokenCount;
    private readonly TimeSpan _refreshPeriod;
    private DateTimeOffset? _lastRefresh;
    private int _currentCount;
    private readonly object _lock = new object();

    public TokenBucketRateLimiter(int tokenCount, TimeSpan refresh)
    {
        _currentCount = _tokenCount = tokenCount.Assert(x => x > 0, $"{nameof(tokenCount)} is not 1 or greater");
        _refreshPeriod = refresh;
    }

    /// <inheritdoc/>
    public int GetAvailablePermits() => _tokenCount;
    public int GetCurrentPermits() => _currentCount;

    public bool TryGetPermit()
    {
        lock (_lock)
        {
            if (_lastRefresh == null || DateTimeOffset.UtcNow > _lastRefresh + _refreshPeriod)
            {
                _lastRefresh = DateTimeOffset.UtcNow;
                _currentCount = _tokenCount;
            }

            return Interlocked.Decrement(ref _currentCount) >= 0;
        }
    }
}
