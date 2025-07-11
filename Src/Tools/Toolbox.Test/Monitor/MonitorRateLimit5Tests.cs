using System.Diagnostics;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Monitor;

public class MonitorRateLimit5Tests
{

    [Fact]
    public async Task RateLimit5()
    {
        const int count = 20;
        var list = new Sequence<(double tps, bool isOver, TimeSpan elapsed)>();

        MonitorRate subject = new MonitorRate(TimeSpan.FromSeconds(1), 5);
        var timestamp = Stopwatch.GetTimestamp();

        foreach (var item in Enumerable.Range(0, count))
        {
            await subject.RecordEventAsync();

            double tps = subject.Stats.Tps;
            list += (tps, subject.Stats.IsOverThreshold, Stopwatch.GetElapsedTime(timestamp));
        }

        TimeSpan elapsed = Stopwatch.GetElapsedTime(timestamp);
        var calTps = count / elapsed.TotalSeconds;
        calTps.Assert(x => x > 0.5, x => $"Calculated TPS={x} should be greater than 5");

        list.Count.Assert(x => x > 0, x => $"Count={x} should be greater than 0");
        list.Where(x => !x.isOver).Count().Assert(x => x > 0, x => $"Over threshold count={x}");
    }

    [Fact]
    public async Task RateLimit5_MaxTps()
    {
        const int count = 20;
        var list = new Sequence<(double tps, bool isOver, TimeSpan elapsed)>();

        MonitorRate subject = new MonitorRate(TimeSpan.FromSeconds(1), 5, 5);
        var timestamp = Stopwatch.GetTimestamp();

        foreach (var item in Enumerable.Range(0, count))
        {
            await subject.RecordEventAsync();

            double tps = subject.Stats.Tps;
            list += (tps, subject.Stats.IsOverThreshold, Stopwatch.GetElapsedTime(timestamp));
        }

        TimeSpan elapsed = Stopwatch.GetElapsedTime(timestamp);
        var calTps = count / elapsed.TotalSeconds;
        calTps.Assert(x => x > 0.5, x => $"Calculated TPS={x} should be greater than 5");

        list.Count.Assert(x => x > 0, x => $"Count={x} should be greater than 0");
        list.Where(x => !x.isOver).Count().Assert(x => x > 0, x => $"Over threshold count={x}");
    }
}
