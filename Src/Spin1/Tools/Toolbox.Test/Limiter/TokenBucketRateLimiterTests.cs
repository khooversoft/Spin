using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Limiter;
using Xunit;

namespace Toolbox.Test.Limiter;

public class TokenBucketRateLimiterTests
{
    [Fact]
    public void GivenBucketSize_WhenNotFilled_ShouldPass()
    {
        var limiter = new TokenBucketRateLimiter(10, TimeSpan.FromMilliseconds(500));

        int acquiredCount = 0;
        int failedCount = 0;
        int count = 20;

        foreach (var item in Enumerable.Range(0, count))
        {
            if (limiter.TryGetPermit()) acquiredCount++; else failedCount++;
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
        }

        acquiredCount.Should().Be(count);
        failedCount.Should().Be(0);
    }

    [Fact]
    public void GivenBucketSize_WhenFilled_ShouldPass()
    {
        var limiter = new TokenBucketRateLimiter(10, TimeSpan.FromMilliseconds(2000));
        limiter.GetAvailablePermits().Should().Be(10);
        limiter.GetCurrentPermits().Should().Be(10);

        int acquiredCount = 0;
        int failedCount = 0;
        int count = 20;

        var sw = Stopwatch.StartNew();

        foreach (var item in Enumerable.Range(0, count))
        {
            if (limiter.TryGetPermit()) acquiredCount++; else failedCount++;
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
        }

        TimeSpan period = sw.Elapsed;

        acquiredCount.Should().BeLessThan(count);
        failedCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GivenPolicy_TimeMeasured_ShouldPass()
    {
        var limiter = new TokenBucketRateLimiter(10, TimeSpan.FromSeconds(2));
        limiter.GetAvailablePermits().Should().Be(10);
        limiter.GetCurrentPermits().Should().Be(10);

        int acquiredCount = 0;
        int failedCount = 0;
        bool running = true;
        var list = new List<(int Stage, TimeSpan Span, int Count, int CurrentCount)>();
        var details = new List<(int Stage, TimeSpan, int CurrentCount)>();

        var sw = Stopwatch.StartNew();

        while (sw.Elapsed < TimeSpan.FromSeconds(5))
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(500));

            if (limiter.TryGetPermit())
            {
                acquiredCount++;
                details.Add((0, sw.Elapsed, limiter.GetCurrentPermits()));
                if (running == false)
                {
                    running = true;
                    list.Add((0, sw.Elapsed, acquiredCount, limiter.GetCurrentPermits()));
                }
            }
            else
            {
                failedCount++;
                details.Add((1, sw.Elapsed, limiter.GetCurrentPermits()));
                if (running == true)
                {
                    running = false;
                    list.Add((1, sw.Elapsed, failedCount, limiter.GetCurrentPermits()));
                }
            }
        }

        TimeSpan period = sw.Elapsed;
    }
}
