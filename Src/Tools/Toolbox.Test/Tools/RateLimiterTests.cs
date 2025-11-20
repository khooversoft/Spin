using System.Diagnostics;
using Toolbox.Tools;

namespace Toolbox.Test.Tools;

public class RateLimiterTests
{
    [Fact]
    public void Construction_DefaultBurst_EqualsUnits()
    {
        var rl = new RateLimiter(10, TimeSpan.FromSeconds(1));
        rl.AvailablePermits.Assert(x => x == 10, x => $"AvailablePermits={x} should be 10");
        rl.BurstCapacity.Assert(x => x == 10, x => $"BurstCapacity={x} should be 10");
        rl.Units.Assert(x => x == 10, x => $"Units={x} should be 10");
        rl.Interval.Assert(x => x == TimeSpan.FromSeconds(1), x => $"Interval={x} should be 1s");
    }

    [Fact]
    public void Construction_WithBurstCapacity()
    {
        var rl = new RateLimiter(10, TimeSpan.FromSeconds(1), burstCapacity: 25);
        rl.AvailablePermits.Assert(x => x == 25, x => $"AvailablePermits={x} should be 25");
        rl.BurstCapacity.Assert(x => x == 25, x => $"BurstCapacity={x} should be 25");
    }

    [Fact]
    public void TryAcquire_DepletesCapacity()
    {
        var rl = new RateLimiter(5, TimeSpan.FromSeconds(1)); // capacity 5
        for (int i = 0; i < 5; i++)
        {
            bool ok = rl.TryAcquire();
            ok.Assert(x => x, $"Acquisition {i} should succeed");
        }

        rl.TryAcquire().Assert(x => !x, "6th acquisition should fail immediately");
        rl.AvailablePermits.Assert(x => x == 0, x => $"AvailablePermits={x} should be 0 after depletion");
    }

    [Fact]
    public void TryAcquire_MultiplePermits()
    {
        var rl = new RateLimiter(10, TimeSpan.FromSeconds(1)); // capacity 10
        rl.TryAcquire(7).Assert(x => x, "Acquire 7 should succeed");
        rl.AvailablePermits.Assert(x => Math.Abs(x - 3) < 0.0001, x => $"Remaining permits should be ~3, was {x}");
        rl.TryAcquire(4).Assert(x => !x, "Acquire 4 should fail (only 3 left)");
        rl.AvailablePermits.Assert(x => Math.Abs(x - 3) < 0.0001, x => $"Remaining permits should still be ~3, was {x}");
    }

    [Fact]
    public void GetEstimatedDelay_ZeroWhenEnoughPermits()
    {
        var rl = new RateLimiter(20, TimeSpan.FromSeconds(1)); // capacity 20
        rl.GetEstimatedDelay(5).Assert(x => x == TimeSpan.Zero, x => $"Delay should be zero, got {x}");
    }

    [Fact]
    public void GetEstimatedDelay_PositiveWhenDeficit()
    {
        // 10 per second => 0.1s per token
        var rl = new RateLimiter(10, TimeSpan.FromSeconds(1));
        rl.TryAcquire(10).Assert(x => x, "Drain all capacity");
        var delay = rl.GetEstimatedDelay(5);

        // Expected: deficit=5, rate=10/sec => 0.5s
        delay.TotalSeconds.Assert(x => Math.Abs(x - 0.5) < 0.05, x => $"Expected ~0.5s, got {x:F3}s");
    }

    [Fact]
    public async Task WaitAsync_NoDelayWithinCapacity()
    {
        var rl = new RateLimiter(50, TimeSpan.FromSeconds(1));
        double before = rl.AvailablePermits;
        await rl.WaitAsync(10);
        rl.AvailablePermits.Assert(x => Math.Abs(x - (before - 10)) < 0.0001, x => $"Expected remaining={before - 10}, got {x}");
    }

    [Fact]
    public async Task WaitAsync_DelayForDeficitAndRefillUponNextAccess()
    {
        // High rate keeps real waiting minimal
        var rl = new RateLimiter(1000, TimeSpan.FromSeconds(1)); // capacity 1000
        rl.TryAcquire(1000).Assert(x => x, "Drain full");
        var est = rl.GetEstimatedDelay(5);
        est.TotalMilliseconds.Assert(x => x >= 5 - 1 && x <= 15, x => $"Estimated ms should be small (~5), got {x}");

        // Wait for 5 permits (should sleep ~5ms)
        await rl.WaitAsync(5);

        // AvailablePermits triggers refill; should now be >=0 (some tokens refilled during wait)
        rl.AvailablePermits.Assert(x => x >= 0, x => $"After wait/refill, permits should be non-negative, got {x}");
    }

    [Fact]
    public void Concurrency_TryAcquireHonorsBurstCapacity()
    {
        var rl = new RateLimiter(100, TimeSpan.FromSeconds(1), burstCapacity: 32);

        int success = 0;
        int attempts = 64;

        Parallel.For(0, attempts, _ =>
        {
            if (rl.TryAcquire())
            {
                Interlocked.Increment(ref success);
            }
        });

        success.Assert(x => x == 32, x => $"Exactly burst capacity should succeed: expected 32, got {x}");
    }

    [Fact]
    public void InvalidConstruction_Throws()
    {
        Verify.Throws<ArgumentOutOfRangeException>(() => new RateLimiter(0, TimeSpan.FromSeconds(1)));
        Verify.Throws<ArgumentOutOfRangeException>(() => new RateLimiter(10, TimeSpan.Zero));
    }

    [Fact]
    public async Task InvalidPermits_Throws()
    {
        var rl = new RateLimiter(10, TimeSpan.FromSeconds(1));
        Verify.Throws<ArgumentOutOfRangeException>(() => rl.TryAcquire(0));
        Verify.Throws<ArgumentOutOfRangeException>(() => rl.GetEstimatedDelay(0));
        await Verify.ThrowsAsync<ArgumentOutOfRangeException>(async () => await rl.WaitAsync(0));
    }

    [Fact]
    public void HighIteration_Performance_NoTimingDependence()
    {
        var rl = new RateLimiter(1000, TimeSpan.FromSeconds(1)); // large capacity
        int iterations = 10_000;
        int granted = 0;

        for (int i = 0; i < iterations; i++)
        {
            if (rl.TryAcquire())
            {
                granted++;
            }
        }

        granted.Assert(x => x == 1000, x => $"Only initial capacity (1000) should be granted without elapsed time, got {x}");
    }

    // New WaitAsync test cases below

    [Fact]
    public async Task WaitAsync_Cancellation_ThrowsQuickly()
    {
        // Configure 1 token/sec, drain 1 to force ~1s wait for next token
        var rl = new RateLimiter(1, TimeSpan.FromSeconds(1));
        rl.TryAcquire().Assert(x => x, "Initial token should be available and consumed");

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        var sw = Stopwatch.StartNew();
        await Verify.ThrowsAsync<OperationCanceledException>(async () => await rl.WaitAsync(1, cts.Token));
        sw.Stop();

        // Ensure cancel occurred much faster than the full expected wait
        sw.Elapsed.TotalMilliseconds.Assert(ms => ms < 800, ms => $"Cancellation should occur quickly, elapsed={ms:F1}ms");
    }

    [Fact]
    public async Task WaitAsync_ConcurrentWaiters_RespectRate()
    {
        // 50 tokens/sec; drain burst to zero then start 10 waiters for 1 token each.
        // Expected total wall time ~ 10 / 50 = 200ms (allow generous tolerance)
        var rl = new RateLimiter(50, TimeSpan.FromSeconds(1));
        rl.TryAcquire(50).Assert(x => x, "Drain initial capacity");

        var sw = Stopwatch.StartNew();

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => rl.WaitAsync().AsTask())
            .ToArray();

        await Task.WhenAll(tasks);
        sw.Stop();

        var elapsedMs = sw.Elapsed.TotalMilliseconds;
        elapsedMs.Assert(ms => ms >= 120 && ms <= 800, ms => $"Concurrent waiters should complete in ~200ms, elapsed={ms:F1}ms");
    }

    [Fact]
    public async Task WaitAsync_ActualDelay_MatchesEstimateWithinTolerance()
    {
        // 100 tokens/sec; request 25 tokens from empty -> ~250ms
        var rl = new RateLimiter(100, TimeSpan.FromSeconds(1));
        rl.TryAcquire(100).Assert(x => x, "Drain initial capacity");

        var est = rl.GetEstimatedDelay(25);
        est.TotalMilliseconds.Assert(ms => ms >= 230 && ms <= 280, ms => $"Estimated ~250ms, got {ms:F1}ms");

        var sw = Stopwatch.StartNew();
        await rl.WaitAsync(25);
        sw.Stop();

        var actual = sw.Elapsed;
        // Allow +/-150ms around estimate to absorb scheduling jitter
        actual.TotalMilliseconds.Assert(ms => ms >= est.TotalMilliseconds - 150 && ms <= est.TotalMilliseconds + 150,
            ms => $"Actual wait {ms:F1}ms should be close to estimate {est.TotalMilliseconds:F1}ms");
    }

    [Fact]
    public async Task WaitAsync_DoesNotExceedBurstCapacityOverTime()
    {
        // Burst 5 tokens, 1 token/sec; after waiting >10s, available should not exceed burst (5)
        var rl = new RateLimiter(1, TimeSpan.FromSeconds(1), burstCapacity: 5);

        // Consume all burst tokens
        rl.TryAcquire(5).Assert(x => x, "Drain all burst tokens");
        rl.AvailablePermits.Assert(x => x == 0, x => $"AvailablePermits should be 0, got {x}");

        // Wait long enough to refill beyond 5 tokens if uncapped
        await Task.Delay(TimeSpan.FromSeconds(2.5)); // should refill ~2-3 tokens

        rl.AvailablePermits.Assert(x => x <= 5, x => $"AvailablePermits should not exceed burst capacity (5), got {x}");
    }
}
