using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;


public partial class MemoryStore : ITrxProvider
{
    public enum RunState
    {
        Ready,
        TransactionRunning,
    };

    public const string SourceNameText = "memoryStore";
    private TrxSourceRecorder? _recorder;
    private string? _logSequenceNumber;
    private EnumState<RunState> _runState = new(RunState.Ready);

    public string SourceName => SourceNameText;
    public string StoreName => SourceNameText;

    public string? GetLogSequenceNumber() => _logSequenceNumber;
    public void SetLogSequenceNumber(string lsn) => Interlocked.Exchange(ref _logSequenceNumber, lsn.NotEmpty());

    public void AttachRecorder(TrxRecorder trxRecorder) => _recorder = trxRecorder.NotNull().ForSource(SourceName);
    public void DetachRecorder() => _recorder = null;

    public Task<Option> Start()
    {
        _recorder.NotNull("Record not attached");
        _runState.TryMove(RunState.Ready, RunState.TransactionRunning).BeTrue("MemoryStore is already in transaction.");
        return new Option(StatusCode.OK).ToTaskResult();
    }

    public Task<Option> Commit()
    {
        _recorder.NotNull("Record not attached");
        _runState.TryMove(RunState.TransactionRunning, RunState.Ready).BeTrue("MemoryStore is not in transaction.");
        return new Option(StatusCode.OK).ToTaskResult();
    }

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

    public Task<string> GetSnapshot()
    {
        lock (_lock)
        {
            var data = new MemoryStoreSerialization(_store.Values, _logSequenceNumber);
            var json = data.ToJson();
            return json.ToTaskResult();
        }
    }

    public Task<Option> Recovery(IEnumerable<DataChangeRecord> records)
    {
        throw new NotImplementedException();
    }

    public Task<Option> Checkpoint() => new Option(StatusCode.OK).ToTaskResult();

    public Task<Option> Restore(string json)
    {
        lock (_lock)
        {
            var data = json.ToObject<MemoryStoreSerialization>();
            _store.Clear();
            _leaseStore.Clear();

            foreach (var detail in data.DirectoryDetails)
            {
                _store[detail.PathDetail.Path] = detail;
            }

            _logSequenceNumber = data.LogSequenceNumber;
        }

        return new Option(StatusCode.OK).ToTaskResult();
    }

    private bool CanModify => _recorder is null || _runState.IfValue(RunState.TransactionRunning);
    public void TestModify() => CanModify.BeTrue("MemoryStore has a record attached and transaction not started.");
}
