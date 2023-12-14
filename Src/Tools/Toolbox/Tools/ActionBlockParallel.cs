using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Toolbox.Extensions;

namespace Toolbox.Tools;

public static class ActionBlockParallel
{
    public static async Task Run<T>(Func<T, Task> receiver, IEnumerable<T> list, int maxDegree = 5)
    {
        receiver.NotNull();
        list.NotNull();
        maxDegree.Assert(x => x >= 0 && x <= 100, x => $"maxDegree {x} out of range");

        var block = new ActionBlock<T>(receiver, new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = 5
        });

        list.ForEach(x => block.Post(x));
        block.Complete();
        await block.Completion;
    }
}
