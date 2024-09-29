﻿using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.TransactionLog;
using Toolbox.Types;

namespace Toolbox.Graph;

public class CmEdgeChange : IChangeLog
{
    public CmEdgeChange(GraphEdge currentValue, GraphEdge newValue)
    {
        CurrentValue = currentValue.NotNull();
        NewValue = newValue.NotNull();
        (CurrentValue.FromKey == NewValue.FromKey).Assert(x => x == true, "Edge FromKey must be the same");
        (CurrentValue.ToKey == NewValue.ToKey).Assert(x => x == true, "Edge ToKey must be the same");
    }

    public Guid LogKey { get; } = Guid.NewGuid();
    private GraphEdge CurrentValue { get; }
    private GraphEdge NewValue { get; }

    public JournalEntry CreateJournal()
    {
        var dataMap = new Dictionary<string, string?>
        {
            { GraphConstants.Trx.CmType, this.GetType().Name },
            { GraphConstants.Trx.LogKey, LogKey.ToString() },
            { GraphConstants.Trx.NewEdge, NewValue.ToJson() },
            { GraphConstants.Trx.CurrentEdge, CurrentValue.ToJson() },
        };

        var journal = JournalEntry.Create(JournalType.Action, dataMap);
        return journal;
    }

    public Task<Option> Undo(IGraphTrxContext graphContext)
    {
        graphContext.NotNull();

        var pk = CurrentValue.GetPrimaryKey();
        graphContext.Map.Edges[pk] = CurrentValue;
        graphContext.Context.LogInformation("Rollback Edge: restored edge logKey={logKey}, value={value}", LogKey, CurrentValue.ToJson());
        return ((Option)StatusCode.OK).ToTaskResult();
    }
}