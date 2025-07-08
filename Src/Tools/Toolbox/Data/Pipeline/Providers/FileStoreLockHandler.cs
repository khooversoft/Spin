using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class FileStoreLockHandler : IDataProvider, IAsyncDisposable
{
    private readonly ILogger<FileStoreLockHandler> _logger;
    private readonly IFileStore _fileStore;
    private readonly LockDetailCollection _lockDetailCollection;
    private readonly DataPipelineLockConfig _lockConfig;
    private bool disposing = false;

    public FileStoreLockHandler(DataPipelineLockConfig lockConfig, IFileStore fileStore, LockDetailCollection lockDetailCollection, ILogger<FileStoreLockHandler> logger)
    {
        _lockConfig = lockConfig.NotNull();
        _fileStore = fileStore.NotNull();
        _lockDetailCollection = lockDetailCollection.NotNull();
        _logger = logger.NotNull();
    }

    public IDataProvider? InnerHandler { get; set; }
    public DataProviderCounters Counters { get; } = new();

    public async ValueTask DisposeAsync()
    {
        var context = _logger.ToScopeContext();
        Interlocked.Exchange(ref disposing, true);

        await _lockDetailCollection.DisposeAsync().ConfigureAwait(false);
    }

    public async Task<Option<DataPipelineContext>> Execute(DataPipelineContext dataContext, ScopeContext context)
    {
        disposing.Assert(x => !x, "Cannot execute while disposing");
        dataContext.NotNull().Validate().ThrowOnError();
        context = context.With(_logger);

        switch (dataContext.Command)
        {
            case DataPipelineCommand.Append:
            case DataPipelineCommand.Set:
            case DataPipelineCommand.Get:
                LockPathConfig? found = _lockConfig.Paths.Values.FirstOrDefault(x => x.Path.Like(dataContext.Path));

                if (found != null)
                {
                    var processLockedOption = await ProcessLock(dataContext, found.LockMode, context).ConfigureAwait(false);
                    if (processLockedOption.IsError()) return processLockedOption.ToOptionStatus<DataPipelineContext>();
                }
                break;

            case DataPipelineCommand.AcquireLock:
                var acquireOption = await ProcessLock(dataContext, LockMode.Shared, context).ConfigureAwait(false);
                if (acquireOption.IsError()) return acquireOption.ToOptionStatus<DataPipelineContext>();
                return dataContext;

            case DataPipelineCommand.AcquireExclusiveLock:
                var acquireExclusiveOption = await ProcessLock(dataContext, LockMode.Exclusive, context).ConfigureAwait(false);
                if (acquireExclusiveOption.IsError()) return acquireExclusiveOption.ToOptionStatus<DataPipelineContext>();
                return dataContext;

            case DataPipelineCommand.ReleaseLock:
                var releaseOption = await ReleaseLock(dataContext, context).ConfigureAwait(false);
                if (releaseOption.IsError()) return releaseOption.ToOptionStatus<DataPipelineContext>();
                return dataContext;
        }

        var nextOption = await ((IDataProvider)this).NextExecute(dataContext, context).ConfigureAwait(false);
        return nextOption;
    }

    private async Task<Option> ReleaseLock(DataPipelineContext dataContext, ScopeContext context)
    {
        LockDetail? lockDetail = _lockDetailCollection.Contains(dataContext.Path);
        if (lockDetail == null) return StatusCode.NotFound;

        var result = await lockDetail.FileLeasedAccess.Release(context).ConfigureAwait(false);
        if (result.IsError())
        {
            context.LogDebug(
                "Failed to release lock for path={path}, statusCode={statusCode}, error={error}",
                dataContext.Path, result.StatusCode, result.Error
                );
        }

        return result;
    }

    private async Task<Option> ProcessLock(DataPipelineContext dataContext, LockMode lockMode, ScopeContext context)
    {
        LockDetail? lockDetail = _lockDetailCollection.Contains(dataContext.Path);
        if (lockDetail != null) return StatusCode.OK;

        switch (lockMode)
        {
            case LockMode.Shared:
                await Acquire(dataContext, context).ConfigureAwait(false);
                break;

            case LockMode.Exclusive:
                await AcquireExclusive(dataContext, context).ConfigureAwait(false);
                break;

            default: throw new InvalidOperationException($"Unknown lock mode '{lockMode}' for path '{dataContext.Path}'");
        }

        return StatusCode.OK;
    }

    private async Task<Option> Acquire(DataPipelineContext dataContext, ScopeContext context)
    {
        int retry = 3;
        int count = 0;

        while (count++ < retry)
        {
            try
            {
                context.LogDebug("Acquiring shared lock for path={path}", dataContext.Path);
                var lockOption = await _fileStore.File(dataContext.Path).Acquire(_lockConfig.AcquireLockDuration, context).ConfigureAwait(false);
                if (lockOption.IsOk())
                {
                    _lockDetailCollection.Set(new LockDetail(lockOption.Return(), false, _lockConfig.AcquireLockDuration));
                    return StatusCode.OK;
                }

                context.LogDebug("Failed to acquire shared lock, attempt {attempt} of {retry}, statusCode={statusCode}, error={error}",
                    count, retry, lockOption.StatusCode, lockOption.Error);

                await Task.Delay(CalculateRetryDelay(count), context.CancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                context.LogError(ex, "Exception raised acquiring shared lock, count={count} of retry={retry}", count, retry);
                throw;
            }
        }
        return (StatusCode.Conflict, "Cannot acquire shared lock after multiple attempts");
    }

    private async Task<Option> AcquireExclusive(DataPipelineContext dataContext, ScopeContext context)
    {
        try
        {
            context.LogDebug("Acquiring exclusive lock for path={path}", dataContext.Path);
            var lockOption = await _fileStore.File(dataContext.Path).AcquireExclusive(true, context).ConfigureAwait(false);
            if (lockOption.IsOk())
            {
                _lockDetailCollection.Set(new LockDetail(lockOption.Return(), true, TimeSpan.MaxValue));
                return StatusCode.OK;
            }

            context.LogError("Failed to acquire exclusive lock for path={path}", dataContext.Path);
            return (StatusCode.Conflict, "Exclusive lease failed");
        }
        catch (Exception ex)
        {
            context.LogError(ex, "Exception raised acquiring exclusive lock");
            throw;
        }
    }

    private static int CalculateRetryDelay(int retryCount)
    {
        const int baseDelayMs = 500; // Starting delay in milliseconds
        const int maxDelayMs = 30000; // Optional: cap the delay to avoid excessive wait times

        // Exponential backoff: delay = baseDelay * 2^(retryCount - 1)
        int delay = baseDelayMs * (int)Math.Pow(2, retryCount - 1);

        // Optional: cap the delay to a maximum value
        return Math.Min(delay, maxDelayMs);
    }
}
