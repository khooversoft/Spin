using Toolbox.Extensions;
using Toolbox.Journal;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class CmEdgeAdd : IChangeLog
{
    public CmEdgeAdd(GraphEdge newValue) => NewValue = newValue.NotNull();

    public Guid LogKey { get; } = Guid.NewGuid();
    public GraphEdge NewValue { get; }

    public JournalEntry CreateJournal()
    {
        var dataMap = new Dictionary<string, string?>
        {
            { GraphConstants.Trx.CmType, this.GetType().Name },
            { GraphConstants.Trx.LogKey, LogKey.ToString() },
            { GraphConstants.Trx.NewEdge, NewValue.ToJson() },
        };

        var journal = JournalEntry.Create(JournalType.Action, dataMap);
        return journal;
    }

    public Task<Option> Undo(IGraphTrxContext graphContext)
    {
        graphContext.NotNull();

        var pk = NewValue.GetPrimaryKey();
        var removeEdge = graphContext.Map.Edges.Remove(pk);
        if (graphContext.Map.Edges.Remove(pk).IsError())
        {
            graphContext.Context.LogError("Rollback Edge: logKey={logKey}, Failed to remove node key={key}", LogKey, pk);
            return ((Option)(StatusCode.Conflict, $"Failed to remove edge edgeKey={pk}")).ToTaskResult();
        }

        graphContext.Context.LogDebug("Rollback Edge: removed edge logKey={logKey}, Edge edgeKey={key} ", LogKey, pk);
        return ((Option)StatusCode.OK).ToTaskResult();
    }
}
