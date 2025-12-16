using System.Threading.Tasks.Dataflow;

namespace Toolbox.Tools;

public class ActionQueue<T> : IAsyncDisposable
{
    private readonly ActionBlock<T> _actionBlock;

    public ActionQueue(Action<T> action, int maxQueue = 100, int maxWorkers = 1)
    {
        action.NotNull();
        maxQueue.Assert(x => x > 0, "Invalid maxQueue value");
        maxWorkers.Assert(x => x > 0, "Invalid maxWorkers value");

        ExecutionDataflowBlockOptions dataflowBlockOptions = new() { BoundedCapacity = maxQueue, MaxDegreeOfParallelism = maxWorkers };
        _actionBlock = new ActionBlock<T>(action, dataflowBlockOptions);
    }

    public ActionQueue(Func<T, Task> action, int maxQueue = 100, int maxWorkers = 1)
    {
        action.NotNull();
        maxQueue.Assert(x => x > 0, "Invalid maxQueue value");
        maxWorkers.Assert(x => x > 0, "Invalid maxWorkers value");

        ExecutionDataflowBlockOptions dataflowBlockOptions = new() { BoundedCapacity = maxQueue, MaxDegreeOfParallelism = maxWorkers };
        _actionBlock = new ActionBlock<T>(action, dataflowBlockOptions);
    }

    public int InputCount => _actionBlock.InputCount;

    public bool IsCompleted => _actionBlock.Completion.IsCompleted;

    public Task<bool> SendAsync(T item) => _actionBlock.SendAsync(item);

    public Task<bool> SendAsync(T item, CancellationToken cancellationToken) => _actionBlock.SendAsync(item, cancellationToken);

    public async Task<bool> SendAsync(IEnumerable<T> items, CancellationToken cancellationToken = default)
    {
        items.NotNull();

        foreach (var item in items)
        {
            var result = await _actionBlock.SendAsync(item, cancellationToken);
            if (!result) return result;
        }

        return true;
    }

    public bool Post(T item) => _actionBlock.Post(item);

    public bool Post(IEnumerable<T> items)
    {
        items.NotNull();

        foreach (var item in items)
        {
            var result = _actionBlock.Post(item);
            if (!result) return result;
        }

        return true;
    }

    public void Close()
    {
        _actionBlock.Complete();
        _actionBlock.Completion.Wait();
    }

    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        _actionBlock.Complete();
        await _actionBlock.Completion.WaitAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync();
        GC.SuppressFinalize(this);
    }
}
