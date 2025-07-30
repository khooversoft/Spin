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
    private readonly LockManager _lockManager;
    private readonly DataPipelineLockConfig _lockConfig;
    private bool disposing = false;

    public FileStoreLockHandler(DataPipelineLockConfig lockConfig, IFileStore fileStore, LockManager lockManager, ILogger<FileStoreLockHandler> logger)
    {
        _lockConfig = lockConfig.NotNull();
        _fileStore = fileStore.NotNull();
        _lockManager = lockManager.NotNull();
        _logger = logger.NotNull();
    }

    public IDataProvider? InnerHandler { get; set; }
    public DataProviderCounters Counters { get; } = new();

    public async ValueTask DisposeAsync()
    {
        var context = _logger.ToScopeContext();
        Interlocked.Exchange(ref disposing, true);

        await _lockManager.DisposeAsync();
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
                var found = _lockConfig.HasLockMode(dataContext.Path, context);

                if (found.IsOk())
                {
                    var processLockedOption = await _lockManager.ProcessLock(dataContext.Path, found.Return(), context);
                    if (processLockedOption.IsError()) return processLockedOption.ToOptionStatus<DataPipelineContext>();
                }
                break;

            case DataPipelineCommand.AcquireLock:
                var acquireOption = await _lockManager.ProcessLock(dataContext.Path, LockMode.Shared, context);
                if (acquireOption.IsError()) return acquireOption.ToOptionStatus<DataPipelineContext>();
                return dataContext;

            case DataPipelineCommand.AcquireExclusiveLock:
                var acquireExclusiveOption = await _lockManager.ProcessLock(dataContext.Path, LockMode.Exclusive, context);
                if (acquireExclusiveOption.IsError()) return acquireExclusiveOption.ToOptionStatus<DataPipelineContext>();
                return dataContext;

            case DataPipelineCommand.ReleaseLock:
                var releaseOption = await _lockManager.ReleaseLock(dataContext.Path, context);
                if (releaseOption.IsError()) return releaseOption.ToOptionStatus<DataPipelineContext>();
                return dataContext;
        }

        var nextOption = await ((IDataProvider)this).NextExecute(dataContext, context);
        return nextOption;
    }
}
