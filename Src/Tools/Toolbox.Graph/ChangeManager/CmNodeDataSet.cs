using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.TransactionLog;
using Toolbox.Types;

namespace Toolbox.Graph;

public class CmNodeDataSet : IChangeLog
{
    public CmNodeDataSet(string fileId, DataETag? oldData = null)
    {
        FileId = fileId.NotEmpty();
        OldData = oldData;
    }

    public Guid LogKey { get; } = Guid.NewGuid();
    public string FileId { get; }
    public DataETag? OldData { get; }


    public JournalEntry CreateJournal()
    {
        var dataMap = new Dictionary<string, string?>
        {
            { GraphConstants.Trx.CmType, this.GetType().Name },
            { GraphConstants.Trx.LogKey, LogKey.ToString() },
            { GraphConstants.Trx.FileId, FileId.ToString() },
            { GraphConstants.Trx.CurrentData, OldData?.ToJson() },
        };

        var journal = JournalEntry.Create(JournalType.Action, dataMap);
        return journal;
    }

    public async Task<Option> Undo(IGraphTrxContext graphContext)
    {
        if (OldData == null)
        {
            var deleteOption = await graphContext.FileStore.Delete(FileId, graphContext.Context);
            return deleteOption.LogStatus(graphContext.Context, $"Undo - delete fileId={FileId}");
        }

        var writeOption = await graphContext.FileStore.Set(FileId, OldData.Value, graphContext.Context);

        return writeOption
            .LogStatus(graphContext.Context, $"Undo - Rollback to oldData for fileId={FileId}")
            .ToOptionStatus();
    }
}
