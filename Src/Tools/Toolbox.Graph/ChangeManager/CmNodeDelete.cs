//using Toolbox.Data;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph;

//public record CmNodeDelete : IChangeLog
//{
//    public CmNodeDelete(GraphNode oldValue) => CurrentValue = oldValue.NotNull();

//    public Guid LogKey { get; } = Guid.NewGuid();
//    public GraphNode CurrentValue { get; }

//    public JournalEntry CreateJournal()
//    {
//        var dataMap = new Dictionary<string, string?>
//        {
//            { GraphConstants.Trx.CmType, this.GetType().Name },
//            { GraphConstants.Trx.LogKey, LogKey.ToString() },
//            { GraphConstants.Trx.CurrentNode, CurrentValue.ToJson() },
//        };

//        var journal = JournalEntry.Create(JournalType.Action, dataMap);
//        return journal;
//    }

//    public Task<Option> Undo(IGraphTrxContext graphContext)
//    {
//        graphContext.NotNull();

//        graphContext.Map.Nodes[CurrentValue.Key] = CurrentValue;
//        graphContext.Context.LogDebug("Rollback: restored node logKey={logKey}, Node key={key}, value={value}", LogKey, CurrentValue.Key, CurrentValue.ToJson());
//        return ((Option)StatusCode.OK).ToTaskResult();
//    }
//}
