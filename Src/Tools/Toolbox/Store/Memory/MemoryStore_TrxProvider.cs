using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;


public partial class MemoryStore : ITrxProvider
{
    public const string SourceNameText = "memoryStore";
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
                entry.After.NotNull("After value must be present for add operation.");
                _store.TryRemove(entry.ObjectId, out var _);
                break;

            case ChangeOperation.Delete:
                entry.Before.NotNull("After value must be present for add operation.");
                _store[entry.ObjectId] = entry.Before.ToObject<DirectoryDetail>();
                break;

            case ChangeOperation.Update:
                entry.Before.NotNull("Before value must be present for update operation.");
                _store[entry.ObjectId] = entry.Before.ToObject<DirectoryDetail>();
                break;
        }

        return new Option(StatusCode.OK).ToTaskResult();
    }
}