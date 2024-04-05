using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class ChangeLog
{
    private ConcurrentStack<IChangeLog> _commands = new();
    private readonly GraphChangeContext _graphChangeContext;

    public ChangeLog(GraphChangeContext graphChangeContext) => _graphChangeContext = graphChangeContext.NotNull();

    public Guid ChangeLogKey { get; } = Guid.NewGuid();

    public void Push(IChangeLog changeLog, GraphChangeContext graphContext)
    {
        changeLog.NotNull(nameof(changeLog));

        graphContext.Context.Location().LogInformation("Pushing changeLog: changeLogKey={changeLogKey}, logKey={logKey}", ChangeLogKey, changeLog.LogKey);
        _commands.Push(changeLog);
    }

    public void Rollback()
    {
        _graphChangeContext.Context.Location().LogInformation("Rollback all: changeLogKey={changeLogKey}, {count} commands", ChangeLogKey, _commands.Count);

        while (_commands.TryPop(out var item))
        {
            item.Undo(_graphChangeContext);
        }
    }

    public IReadOnlyList<IChangeLog> GetCommands() => _commands.ToArray();
}
