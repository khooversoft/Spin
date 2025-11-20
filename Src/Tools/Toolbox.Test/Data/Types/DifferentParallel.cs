using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Threading.Tasks.Dataflow;
using Toolbox.Tools;

namespace Toolbox.Test.Data.Types;

public class DifferentParallel
{
    [Fact]
    public async Task TestParallelFor()
    {
        const int iterations = 1000;
        var receivce = new ConcurrentQueue<int>();

        var block = new ActionBlock<Func<int>>(async item =>
        {
            await Task.Delay(RandomNumberGenerator.GetInt32(1, 5));
            receivce.Enqueue(item());
        },
        new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 }
        );

        foreach (var item in Enumerable.Range(0, iterations))
        {
            await block.SendAsync(() => item);
            await block.SendAsync(() => item + 100_000);
        }

        block.Complete();
        await block.Completion;

        receivce.Count.Be(iterations * 2);
    }
}
