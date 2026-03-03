using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;


public partial class MemoryStore : ITrxProvider
{
    public const string SourceNameText = "memoryStore";
    private TrxSourceRecorder? _recorder;
    private string? _logSequenceNumber;

    public string SourceName => SourceNameText;
    public string StoreName => SourceNameText;

    public void AttachRecorder(TrxRecorder trxRecorder) => _recorder = trxRecorder.NotNull().ForSource(SourceName);
    public void DetachRecorder() => _recorder = null;

    public Task<Option> Start()
    {
        _recorder.NotNull("Record not attached");
        return new Option(StatusCode.OK).ToTaskResult();
    }

    public Task<Option> Commit(DataChangeRecord dcr)
    {
        dcr.NotNull();
        _recorder.NotNull("Record not attached");

        _logSequenceNumber = dcr.GetLastLogSequenceNumber();
        return new Option(StatusCode.OK).ToTaskResult();
    }

    public Task<Option> Rollback(DataChangeEntry entry)
    {
        switch (entry.Action)
        {
            case ActionOperator.Add:
                _store.TryRemove(entry.ObjectId, out var _);
                break;

            case ActionOperator.Delete:
                entry.Before.NotNull("After value must be present for add operation.");
                _store[entry.ObjectId] = entry.Before.ToObject<DirectoryDetail>();
                break;

            case ActionOperator.Update:
                entry.Before.NotNull("Before value must be present for update operation.");
                _store[entry.ObjectId] = entry.Before.ToObject<DirectoryDetail>();
                break;
        }

        return new Option(StatusCode.OK).ToTaskResult();
    }

    public Task<Option> Recovery(TrxRecoveryScope trxRecoveryScope)
    {
        trxRecoveryScope.NotNull();

        var storeLastLsn = LogSequenceNumber.Parse(_logSequenceNumber);

        foreach (var record in trxRecoveryScope.Records)
        {
            var lsn = record.GetLastLogSequenceNumber().NotNull().Func(x => LogSequenceNumber.Parse(x));
            if (lsn <= storeLastLsn) continue;

            foreach (var entry in record.Entries)
            {
                switch (entry.Action)
                {
                    case ActionOperator.Add:
                        entry.After.NotNull("After value must be present for add operation.");
                        _store.TryAdd(entry.ObjectId, entry.After.ToObject<DirectoryDetail>());
                        break;

                    case ActionOperator.Delete:
                        _store.TryRemove(entry.ObjectId, out var _).BeTrue();
                        break;

                    case ActionOperator.Update:
                        entry.After.NotNull("Before value must be present for update operation.");
                        _store[entry.ObjectId] = entry.After.ToObject<DirectoryDetail>();
                        break;
                }

                _logSequenceNumber = entry.LogSequenceNumber;
            }
        }

        return new Option(StatusCode.OK).ToTaskResult();
    }

    public Task<Option> Checkpoint() => new Option(StatusCode.OK).ToTaskResult();

    public Task<Option> Restore(string json)
    {
        lock (_lock)
        {
            var data = json.ToObject<MemoryStoreSerialization>();
            _store.Clear();
            _leaseStore.Clear();

            foreach (var detail in data.DirectoryDetails) _store[detail.PathDetail.Path] = detail;
            _logSequenceNumber = data.LogSequenceNumber;
        }

        return new Option(StatusCode.OK).ToTaskResult();
    }

    public Option<string> GetLogSequenceNumber() => _logSequenceNumber switch { null => StatusCode.NotFound, var lsn => lsn, };
    public void SetLogSequenceNumber(string lsn) => Interlocked.Exchange(ref _logSequenceNumber, lsn.NotEmpty());

    public Task<Option<string>> GetSnapshot()
    {
        lock (_lock)
        {
            var data = new MemoryStoreSerialization(_store.Values, _logSequenceNumber);
            var json = data.ToJson();
            return new Option<string>(json).ToTaskResult();
        }
    }
}
