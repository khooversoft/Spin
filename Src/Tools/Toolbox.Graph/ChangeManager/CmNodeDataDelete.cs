using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.TransactionLog;
using Toolbox.Types;

namespace Toolbox.Graph;

public class CmNodeDataDelete : IChangeLog
{
    public CmNodeDataDelete(string fileId, DataETag oldData)
    {
        FileId = fileId.NotEmpty();
        OldData = oldData;
    }

    public Guid LogKey { get; } = Guid.NewGuid();
    public string FileId { get; }
    public DataETag OldData { get; }


    public JournalEntry CreateJournal()
    {
        var dataMap = new Dictionary<string, string?>
        {
            { GraphConstants.Trx.CmType, this.GetType().Name },
            { GraphConstants.Trx.LogKey, LogKey.ToString() },
            { GraphConstants.Trx.FileId, FileId.ToString() },
            { GraphConstants.Trx.CurrentData, OldData.ToJson() },
        };

        var journal = JournalEntry.Create(JournalType.Action, dataMap);
        return journal;
    }

    public async Task<Option> Undo(IGraphTrxContext graphContext)
    {
        var writeOption = await graphContext.FileStore.Set(FileId, OldData, graphContext.Context);
        writeOption.LogStatus(graphContext.Context, $"Undo - Rollback to oldData for fileId={FileId}").ToOptionStatus();
        return StatusCode.OK;
    }
}
