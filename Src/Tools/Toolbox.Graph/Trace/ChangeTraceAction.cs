using System.Threading.Tasks.Dataflow;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph;

public class ChangeTraceAction : IChangeTrace
{
    private readonly Func<string, Task> _action;
    private readonly int _maxSize;
    private readonly ActionBlock<ChangeTrx> _forward;
    private readonly object _lock = new object();

    public ChangeTraceAction(Func<string, Task> action, int maxSize = 100_000)
    {
        _forward = new ActionBlock<ChangeTrx>(Enqueue, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });
        _action = action.NotNull();
        _maxSize = maxSize;
    }

    public void Log(ChangeTrx trx) => _forward.Post(trx);
    public Task LogAsync(ChangeTrx trx) => _forward.SendAsync(trx);

    private Task Enqueue(ChangeTrx trx)
    {
        string json = trx.ToJson();
        return _action(json);
    }
}

