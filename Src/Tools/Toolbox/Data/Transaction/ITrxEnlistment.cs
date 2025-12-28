using Toolbox.Types;

namespace Toolbox.Data;

public interface ITrxProvider
{
    public string SourceName { get; }
    void AttachRecorder(TrxRecorder trxRecorder);
    void DetachRecorder();

    public Task<Option> Start();
    public Task<Option> Commit();
    public Task<Option> Rollback(DataChangeEntry dataChangeRecord);
}
