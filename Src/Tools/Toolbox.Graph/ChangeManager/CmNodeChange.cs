﻿using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.TransactionLog;
using Toolbox.Types;

namespace Toolbox.Graph;

public class CmNodeChange : IChangeLog
{
    public CmNodeChange(GraphNode currentValue, GraphNode newValue)
    {
        CurrentValue = currentValue.NotNull();
        NewValue = newValue.NotNull();
        (CurrentValue.Key == NewValue.Key).Assert(x => x == true, "Node Key must be the same");
    }

    public Guid LogKey { get; } = Guid.NewGuid();
    public GraphNode CurrentValue { get; }
    public GraphNode NewValue { get; }

    public JournalEntry CreateJournal()
    {
        var dataMap = new Dictionary<string, string?>
        {
            { GraphConstants.Trx.ChangeType, this.GetType().Name },
            { GraphConstants.Trx.NewNode, NewValue.ToJson() },
            { GraphConstants.Trx.CurrentNode, CurrentValue.ToJson() },
            { GraphConstants.Trx.LogKey, LogKey.ToString() },
            { GraphConstants.Trx.Primarykey, NewValue.Key.ToString() }
        };

        var journal = JournalEntry.Create(JournalType.Action, dataMap);
        return journal;
    }

    public Task<Option> Undo(IGraphTrxContext graphContext)
    {
        graphContext.NotNull();

        graphContext.Map.Nodes[CurrentValue.Key] = CurrentValue;
        graphContext.Context.LogInformation("Rollback Node: restored node logKey={logKey}, key={key}, value={value}", LogKey, CurrentValue.Key, CurrentValue.ToJson());
        return ((Option)StatusCode.OK).ToTaskResult();
    }
}
