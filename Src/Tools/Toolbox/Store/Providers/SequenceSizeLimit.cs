using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Telemetry;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class SequenceSizeLimit<T>
{
    private readonly ILogger<SequenceSizeLimit<T>> _logger;
    private readonly SequentialAsyncQueue _queue;
    private readonly ISequenceStore<T> _sequenceStore;
    private readonly SequenceSizeLimitOption<T> _option;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ITelemetryCounter<long>? _signalCounter;
    private readonly ITelemetryCounter<long>? _deleteCounter;
    private DateTime? _lastTime;

    public SequenceSizeLimit(ISequenceStore<T> sequenceStore, SequenceSizeLimitOption<T> option, IServiceProvider serviceProvider, ILogger<SequenceSizeLimit<T>> logger, ITelemetry? telemetry = null)
    {
        _sequenceStore = sequenceStore.NotNull();
        _option = option.NotNull().Action(x => x.Validate().ThrowOnError());
        _logger = logger.NotNull();

        _queue = ActivatorUtilities.CreateInstance<SequentialAsyncQueue>(serviceProvider.NotNull(), 10);
        _signalCounter = telemetry?.CreateCounter<long>("SequenceSizeLimit.signalCounter", "Current signal count", unit: "count");
        _deleteCounter = telemetry?.CreateCounter<long>("SequenceSizeLimit.deleteCounter", "Current delete count", unit: "count");
    }

    public async Task SignalChange(bool force = false)
    {
        _logger.LogTrace("Enqueuing size limit check for maxItems={maxItems}", _option.MaxItems);
        _signalCounter?.Increment();

        if (force || _option.CheckInterval == TimeSpan.Zero)
        {
            // No deferred processing, run immediately
            await InternalCleanup();
            return;
        }

        await _semaphore.WaitAsync();
        try
        {
            if (_lastTime != null && (_lastTime.Value + _option.CheckInterval) > DateTime.UtcNow)
            {
                _logger.LogTrace("Size limit check already performed recently, skipping");
                return;
            }

            _lastTime = DateTime.UtcNow;
            await _queue.Send(InternalCleanup);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task Cleanup()
    {
        await _queue.Drain();
        await InternalCleanup();
    }

    private async Task InternalCleanup()
    {
        _logger.LogDebug("Deleting excess items for key={key}", _option.Key);

        // Get full file list for sequence
        var list = await _sequenceStore.GetDetails(_option.Key);
        if (list.Count <= _option.MaxItems)
        {
            _logger.LogDebug("No excess items to delete for key={key}", _option.Key);
            return;
        }

        var deleteList = list.Take(list.Count - _option.MaxItems).ToArray();

        foreach (var item in deleteList)
        {
            _logger.LogDebug("Deleting excess item key={key}, path={path}", _option.Key, item.Path);

            var delete = await _sequenceStore.DeleteItem(item.Path);
            if (delete.IsError())
            {
                _logger.LogError("Error deleting excess item key={key}, path={path}, statusCode={statusCode}, error={error}", _option.Key, item.Path, delete.StatusCode, delete.Error);
                continue;
            }

            _deleteCounter?.Increment();
        }

        _logger.LogDebug("Deleted excess items for key={key}, count={count}", _option.Key, deleteList.Length);
    }
}
