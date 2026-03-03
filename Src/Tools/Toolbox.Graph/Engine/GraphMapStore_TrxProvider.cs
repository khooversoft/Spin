using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public partial class GraphMapStore : ITrxProvider
{
    public const string SourceNameText = "graphMapStore";
    private TrxSourceRecorder? _recorder;
    private string? _logSequenceNumber;
    private readonly object _lock = new object();

    public string SourceName => SourceNameText;
    public string StoreName => SourceNameText;
    public TrxSourceRecorder? Recorder => _recorder;

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

    public async Task<Option> Rollback(DataChangeEntry entry)
    {
        _map.NotNull("Map is not set.");

        switch (entry.SourceName)
        {
            case ChangeSource.Node:
                await NodeCompensate.Compensate(_map, entry, _logger);
                break;

            case ChangeSource.Edge:
                entry.Before.NotNull("After value must be present for add operation.");
                await EdgeCompensate.Compensate(_map, entry, _logger);
                break;

            case ChangeSource.Data:
                entry.Before.NotNull("Before value must be present for update operation.");
                await DataCompensate.Compensate(_map, entry, _dataFileClient, _logger);
                break;
        }

        return StatusCode.OK;
    }

    public async Task<Option> Recovery(TrxRecoveryScope trxRecoveryScope)
    {
        _map.NotNull("Map is not set.");
        trxRecoveryScope.NotNull();

        var storeLastLsn = LogSequenceNumber.Parse(_logSequenceNumber);

        foreach (var record in trxRecoveryScope.Records)
        {
            var lsn = record.GetLastLogSequenceNumber().NotNull().Func(x => LogSequenceNumber.Parse(x));
            if (lsn <= storeLastLsn) continue;

            foreach (var entry in record.Entries)
            {
                switch (entry.SourceName)
                {
                    case ChangeSource.Node:
                        entry.After.NotNull("After value must be present for add operation.");
                        await NodeBuild.Build(_map, entry, _logger);
                        break;

                    case ChangeSource.Edge:
                        await EdgeBuild.Build(_map, entry, _logger);
                        break;

                    case ChangeSource.Data:
                        entry.After.NotNull("Before value must be present for update operation.");
                        await DataBuild.Build(_map, entry, _dataFileClient, _logger);
                        break;
                }

                _logSequenceNumber = entry.LogSequenceNumber;
            }
        }

        return StatusCode.OK;
    }

    public async Task<Option> Checkpoint()
    {
        _map.NotNull("Map is not set.");

        var setOption = await _graphMapStore.Set(GraphConstants.GraphMap.Key, _map.ToSerialization());
        if (setOption.IsError()) return _logger.LogStatus(setOption, "Failed to save graph map").ToOptionStatus();

        return StatusCode.OK;
    }

    public async Task<Option> Restore(string json)
    {
        await _semaphore.WaitAsync();

        try
        {
            var data = json.ToObject<GraphSerialization>();
            var map = data.FromSerialization(_serviceProvider);
            _map = map.NotNull();

            return StatusCode.OK;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public Option<string> GetLogSequenceNumber() => _logSequenceNumber switch { null => StatusCode.NotFound, var lsn => lsn, };
    public void SetLogSequenceNumber(string lsn) => Interlocked.Exchange(ref _logSequenceNumber, lsn.NotEmpty());
}
