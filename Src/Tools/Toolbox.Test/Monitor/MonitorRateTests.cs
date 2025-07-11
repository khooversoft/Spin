using System.Diagnostics;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Monitor;

public class MonitorRateTests
{
    [Fact]
    public void SimpleTest()
    {
        MonitorRate subject = new MonitorRate(TimeSpan.FromSeconds(1), 1.0f);
        subject.RecordEvent();
        subject.RecordEvent();
        subject.RecordEvent();

        Thread.Sleep(TimeSpan.FromMilliseconds(500));
        subject.Stats.Tps.Assert(x => x > 0, x => $"TPS={x} should be greater than 0");
        subject.Stats.IsOverThreshold.BeTrue($"TPS={subject.Stats.Tps} threshold");
    }

    [Fact]
    public async Task LargerWindowTest()
    {
        MonitorRate subject = new MonitorRate(TimeSpan.FromSeconds(10), 1.0f);
        subject.RecordEvent();
        subject.RecordEvent();
        subject.RecordEvent();

        await Task.Delay(TimeSpan.FromSeconds(1));
        subject.Stats.Tps.Assert(x => x > 0, "TPS should be greater than 0");
        subject.Stats.IsOverThreshold.BeFalse($"TPS={subject.Stats.Tps} threshold");
    }


    [Fact]
    public async Task RateLimit()
    {
        const int count = 10;
        var list = new Sequence<(double tps, bool isOver, TimeSpan elapsed)>();

        MonitorRate subject = new MonitorRate(TimeSpan.FromSeconds(1), 1.0f);
        var timestamp = Stopwatch.GetTimestamp();

        foreach (var item in Enumerable.Range(0, count))
        {
            subject.RecordEvent();
            await subject.WhenUnder();

            double tps = subject.Stats.Tps;
            list += (tps, subject.Stats.IsOverThreshold, Stopwatch.GetElapsedTime(timestamp));
        }

        TimeSpan elapsed = Stopwatch.GetElapsedTime(timestamp);
        var calTps = 10 / elapsed.TotalSeconds;
        calTps.Assert(x => x > 0.5, x => $"Calculated TPS={x} should be greater than 5");

        list.Count.Assert(x => x > 0, x => $"Count={x} should be greater than 0");
        list.Where(x => !x.isOver).Count().Assert(x => x > 0, x => $"Over threshold count={x}");
    }
}
