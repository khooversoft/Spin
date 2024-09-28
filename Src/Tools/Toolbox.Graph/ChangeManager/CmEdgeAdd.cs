﻿using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.TransactionLog;
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
            { GraphConstants.Trx.ChangeType, this.GetType().Name },
            { GraphConstants.Trx.NewEdge, NewValue.ToJson() },
            { GraphConstants.Trx.LogKey, LogKey.ToString() },
            { GraphConstants.Trx.Primarykey, NewValue.GetPrimaryKey().ToString() }
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
            graphContext.Context.LogError("Rollback Edge: logKey={logKey}, Failed to remove node key={key}", pk);
            return ((Option)(StatusCode.Conflict, $"Failed to remove edge edgeKey={pk}")).ToTaskResult();
        }

        graphContext.Context.LogInformation("Rollback Edge: removed edge logKey={logKey}, Edge edgeKey={key} ", LogKey, pk);
        return ((Option)StatusCode.OK).ToTaskResult();
    }
}
