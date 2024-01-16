using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools;

public static class ActionParallel
{
    public static async Task Run<T>(Func<T, Task> process, IEnumerable<T> list, int maxDegree = 5)
    {
        process.NotNull();
        list.NotNull();
        maxDegree.Assert(x => x >= 0 && x <= 100, x => $"maxDegree {x} out of range");

        var block = new ActionBlock<T>(process, new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = maxDegree
        });

        list.ForEach(x => block.Post(x));
        block.Complete();
        await block.Completion;
    }

    public static IReadOnlyList<Option<TR>> Run<TS, TR>(IEnumerable<TS> list, Func<TS, Option<TR>> process, int maxDegree = 5)
    {
        process.NotNull();
        list.NotNull();

        var queue = new ConcurrentQueue<Option<TR>>();
        var block = new ActionBlock<TS>(blockProcess, new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = maxDegree
        });

        list.ForEach(x => block.Post(x));
        block.Complete();
        block.Completion.GetAwaiter().GetResult();

        return queue.ToArray();

        Task blockProcess(TS value)
        {
            try
            {
                var result = process(value);
                queue.Enqueue(result);
            }
            catch (Exception ex)
            {
                queue.Enqueue((StatusCode.Conflict, ex.ToString()));
            }

            return Task.CompletedTask;
        }
    }

    public static async Task<IReadOnlyList<Option>> RunAsync<T>(IEnumerable<T> list, Func<T, Task<Option>> process, int maxDegree = 5)
    {
        process.NotNull();
        list.NotNull();

        var queue = new ConcurrentQueue<Option>();
        var block = new ActionBlock<T>(blockProcess, new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = maxDegree
        });

        list.ForEach(x => block.Post(x));
        block.Complete();
        await block.Completion;

        return queue.ToArray();

        async Task blockProcess(T value)
        {
            try
            {
                var result = await process(value);
                queue.Enqueue(result);
            }
            catch (Exception ex)
            {
                queue.Enqueue((StatusCode.Conflict, ex.ToString()));
            }
        }
    }

    public static async Task<IReadOnlyList<Option<TR>>> RunAsync<TS, TR>(IEnumerable<TS> list, Func<TS, Task<Option<TR>>> process, int maxDegree = 5)
    {
        process.NotNull();
        list.NotNull();

        var queue = new ConcurrentQueue<Option<TR>>();
        var block = new ActionBlock<TS>(blockProcess, new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = maxDegree
        });

        list.ForEach(x => block.Post(x));
        block.Complete();
        await block.Completion;

        return queue.ToArray();

        async Task blockProcess(TS value)
        {
            try
            {
                var result = await process(value);
                queue.Enqueue(result);
            }
            catch (Exception ex)
            {
                queue.Enqueue((StatusCode.Conflict, ex.ToString()));
            }
        }
    }
}
