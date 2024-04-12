using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class ChangeLog
{
    private ConcurrentStack<IChangeLog> _commands = new();
    private readonly GraphContext _graphChangeContext;

    public ChangeLog(GraphContext graphChangeContext) => _graphChangeContext = graphChangeContext.NotNull();

    public Guid TrxId { get; } = Guid.NewGuid();

    public void Push(IChangeLog changeLog)
    {
        changeLog.NotNull();

        _graphChangeContext.Context.Location().LogInformation("Pushing changeLog: changeLogKey={changeLogKey}, trxId={trxId}", TrxId, changeLog.LogKey);
        _commands.Push(changeLog);
        _graphChangeContext.ChangeTrace?.Log(changeLog.GetChangeTrx(TrxId));
    }

    public void Rollback()
    {
        _graphChangeContext.Context.Location().LogInformation("Rollback all: trxId={trxId}, {count} commands", TrxId, _commands.Count);

        while (_commands.TryPop(out var item))
        {
            item.Undo(_graphChangeContext);
            _graphChangeContext.ChangeTrace?.Log(item.GetUndoChangeTrx(TrxId));
        }
    }

    public IReadOnlyList<IChangeLog> GetCommands() => _commands.ToArray();
}
