using System.Collections.Concurrent;
using Toolbox.Tools;

namespace Toolbox.Graph;

public class ChangeLog
{
    private ConcurrentStack<IChangeLog> _commands = new();
    private readonly IGraphTrxContext _graphContext;

    public ChangeLog(IGraphTrxContext graphContext) => _graphContext = graphContext.NotNull();

    public Guid TrxId { get; } = Guid.NewGuid();

    public void Push(IChangeLog changeLog)
    {
        changeLog.NotNull();
        _commands.Push(changeLog);
    }

    public void Rollback()
    {
        while (_commands.TryPop(out var item))
        {
            item.Undo(_graphContext);
        }
    }

    public IReadOnlyList<IChangeLog> GetCommands() => _commands.ToArray();
}
