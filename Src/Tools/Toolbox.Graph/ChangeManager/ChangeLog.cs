using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.TransactionLog;
using Toolbox.Types;

namespace Toolbox.Graph;


public interface IChangeLog
{
    Guid LogKey { get; }
    JournalEntry CreateJournal();
    Task<Option> Undo(IGraphTrxContext graphContext);
}


public class ChangeLog
{
    private ConcurrentStack<IChangeLog> _commands = new();
    private readonly IGraphTrxContext _graphTrxContext;

    public ChangeLog(IGraphTrxContext graphContext) => _graphTrxContext = graphContext.NotNull();

    public Guid TrxId { get; } = Guid.NewGuid();

    public void Push(IChangeLog changeLog)
    {
        changeLog.NotNull();
        _commands.Push(changeLog);
        _graphTrxContext.Context.LogInformation("Push changeLog={changeLog}", changeLog.LogKey);
    }

    public async Task Rollback()
    {
        _graphTrxContext.Context.LogWarning("Rolling back changeLog");

        while (_commands.TryPop(out var changeLog))
        {
            var result = await changeLog.Undo(_graphTrxContext);
            result.LogStatus(_graphTrxContext.Context, $"Undo changeLog={changeLog}");
            _graphTrxContext.Context.LogWarning("Rolling back changeLog={changeLog}", changeLog.LogKey);
        }
    }

    public IReadOnlyList<IChangeLog> GetCommands() => _commands.ToArray();
}
