using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.TransactionLog;
using Toolbox.Types;

namespace Toolbox.Graph;

public class CmNodeAdd : IChangeLog
{
    public CmNodeAdd(GraphNode newValue) => NewValue = newValue.NotNull();

    public Guid LogKey { get; } = Guid.NewGuid();
    public GraphNode NewValue { get; }

    public JournalEntry CreateJournal()
    {
        var dataMap = new Dictionary<string, string?>
        {
            { GraphConstants.Trx.NewNode, NewValue.ToJson() },
            { GraphConstants.Trx.LogKey, LogKey.ToString() },
            { GraphConstants.Trx.Primarykey, NewValue.Key }
        };

        var journal = JournalEntry.Create(JournalType.Action, dataMap);
        return journal;
    }

    public Task<Option> Undo(IGraphTrxContext graphContext)
    {
        graphContext.NotNull();

        if (graphContext.Map.Nodes.Remove(NewValue.Key).IsError())
        {
            graphContext.Context.LogError("Rollback: logKey={logKey}, Failed to remove node key={key}", LogKey, NewValue.Key);
            return ((Option)(StatusCode.Conflict, $"Failed to remove node key={NewValue.Key}")).ToTaskResult();
        }

        graphContext.Context.LogInformation("Rollback: removed node logKey={logKey}, Node key={key}", LogKey, NewValue.Key);
        return ((Option)StatusCode.OK).ToTaskResult();
    }
}
