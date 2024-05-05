using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class CmNodeDataDelete : IChangeLog
{
    public CmNodeDataDelete(string fileId, DataETag oldData)
    {
        FileId = fileId.NotEmpty();
        OldData = oldData;
    }

    public string FileId { get; }
    public DataETag OldData { get; }

    public Guid LogKey => throw new NotImplementedException();

    public async Task<Option> Undo(IGraphTrxContext graphContext)
    {
        var writeOption = await graphContext.FileStore.Set(FileId, OldData, graphContext.Context);

        return writeOption
            .LogStatus(graphContext.Context, $"Undo - Rollback to oldData for fileId={FileId}")
            .ToOptionStatus();
    }
}
