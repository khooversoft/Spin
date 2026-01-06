using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public partial class DatalakeStore : ITrxProvider
{
    public const string SourceNameText = "datalakeStore";
    private TrxSourceRecorder? _recorder;

    public string SourceName => SourceNameText;

    public void AttachRecorder(TrxRecorder trxRecorder) => _recorder = trxRecorder.NotNull().ForSource(SourceName);
    public void DetachRecorder() => _recorder = null;

    public Task<Option> Start() => new Option(StatusCode.OK).ToTaskResult();
    public Task<Option> Commit() => new Option(StatusCode.OK).ToTaskResult();

    public Task<Option> Rollback(DataChangeEntry entry)
    {
        switch (entry.Action)
        {
            case ChangeOperation.Add:
                entry.After.HasValue.BeTrue("After value must be present for add operation.");
                //_store.TryRemove(entry.ObjectId, out var _);
                break;

            case ChangeOperation.Delete:
                //entry.Before.HasValue.BeTrue("After value must be present for add operation.");
                //_store[entry.ObjectId] = entry.Before!.Value.ToObject<DirectoryDetail>();
                break;

            case ChangeOperation.Update:
                //entry.Before.HasValue.BeTrue("After value must be present for add operation.");
                //_store[entry.ObjectId] = entry.Before!.Value.ToObject<DirectoryDetail>();
                break;
        }

        return new Option(StatusCode.OK).ToTaskResult();
    }
}
