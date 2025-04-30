using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Monitor;

public class MonitorSteadyStateTests
{
    [Fact]
    public async Task SteadyStateTest()
    {
        var list = new Sequence<(double tps, bool isOver)>();

        MonitorRate subject = new MonitorRate(TimeSpan.FromSeconds(1), 1.0f);

        foreach (var item in Enumerable.Range(0, 10))
        {
            subject.RecordEvent();
            await Task.Delay(TimeSpan.FromMilliseconds(500));

            double tps = subject.Stats.Tps;
            list += (tps, subject.Stats.IsOverThreshold);
            tps.Assert(x => x > 0, "TPS should be greater than 0");
        }

        list.Count.Assert(x => x > 0, x => $"Count={x} should be greater than 0");
        list.Where(x => x.isOver).Count().Assert(x => x > 0, x => $"Over threshold count={x}");
    }
}
