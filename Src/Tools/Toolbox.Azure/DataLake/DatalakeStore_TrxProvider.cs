//using Microsoft.Extensions.Logging;
//using Toolbox.Data;
//using Toolbox.Extensions;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Azure;

//public partial class DatalakeStore : ITrxProvider
//{
//    public const string SourceNameText = "datalakeStore";
//    private TrxSourceRecorder? _recorder;

//    public string SourceName => SourceNameText;

//    public void AttachRecorder(TrxRecorder trxRecorder) => _recorder = trxRecorder.NotNull().ForSource(SourceName);
//    public void DetachRecorder() => _recorder = null;

//    public Task<Option> Start() => new Option(StatusCode.OK).ToTaskResult();
//    public Task<Option> Commit(DataChangeRecord dcr) => new Option(StatusCode.OK).ToTaskResult();

//    public async Task<Option> Rollback(DataChangeEntry entry)
//    {
//        switch (entry.Action)
//        {
//            case ChangeOperation.Add:
//                entry.After.NotNull("After value must be present for add operation.");
//                var deleteOption = await this.ForceDelete(entry.ObjectId);
//                if (deleteOption.IsError())
//                {
//                    _logger.LogCritical("Rollback failed to delete objectId: {ObjectId} with error: {Error}", entry.ObjectId, deleteOption.Error);
//                    return deleteOption;
//                }
//                break;

//            case ChangeOperation.Delete:
//                entry.Before.NotNull("After value must be present for add operation.");
//                var setOption = await this.ForceSet(entry.ObjectId, entry.Before);
//                if (setOption.IsError())
//                {
//                    _logger.LogCritical("Rollback failed to set objectId: {ObjectId} with error: {Error}", entry.ObjectId, setOption.Error);
//                    return setOption.ToOptionStatus();
//                }
//                break;

//            case ChangeOperation.Update:
//                entry.Before.NotNull("Before value must be present for update operation.");
//                var updateOption = await this.ForceSet(entry.ObjectId, entry.Before);
//                if (updateOption.IsError())
//                {
//                    _logger.LogCritical("Rollback failed to set objectId: {ObjectId} with error: {Error}", entry.ObjectId, updateOption.Error);
//                    return updateOption.ToOptionStatus();
//                }
//                break;
//        }

//        return StatusCode.OK;
//    }

//    public async Task<Option> Recovery(TrxRecoveryScope trxRecoveryScope)
//    {
//        trxRecoveryScope.NotNull();

//        var storeLastLsn = trxRecoveryScope.LogSequenceNumber ?? Lsn.Default;

//        foreach (var record in trxRecoveryScope.Records)
//        {
//            var lsn = record.GetLastLogSequenceNumber().NotNull().Func(x => LogSequenceNumber.Parse(x));
//            if (lsn <= storeLastLsn) continue;

//            foreach (var entry in record.Entries)
//            {
//                switch (entry.Action)
//                {
//                    case ChangeOperation.Add:
//                        entry.After.NotNull("After value must be present for add operation.");
//                        (await Add(entry.ObjectId, entry.After)).BeOk();
//                        break;

//                    case ChangeOperation.Delete:
//                        (await Delete(entry.ObjectId)).BeOk();
//                        break;

//                    case ChangeOperation.Update:
//                        entry.After.NotNull("Before value must be present for update operation.");
//                        await Set(entry.ObjectId, entry.After);
//                        break;
//                }
//            }
//        }

//        return StatusCode.OK;
//    }

//    public Task<Option> Checkpoint() => new Option(StatusCode.OK).ToTaskResult();
//    public Option<string> GetLogSequenceNumber() => StatusCode.ServiceUnavailable;
//    public Task<Option> Restore(string json) => new Option(StatusCode.ServiceUnavailable).ToTaskResult();
//    public void SetLogSequenceNumber(string lsn) { }
//}
