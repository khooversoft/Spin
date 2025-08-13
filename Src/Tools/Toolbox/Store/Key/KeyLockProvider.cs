using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class KeyLockProvider<T> : IKeyStore<T>
{
    private readonly IFileSystem<T> _fileSystem;
    private readonly ILogger<KeyCacheProvider<T>> _logger;
    private readonly LockMode _lockMode;
    private readonly LockManager _lockManager;

    public KeyLockProvider(LockMode lockMode, IFileSystem<T> fileSystem, LockManager lockManager, ILogger<KeyCacheProvider<T>> logger)
    {
        _lockMode = lockMode.Action(x => x.IsEnumValid().BeTrue());
        _fileSystem = fileSystem.NotNull();
        _lockManager = lockManager.NotNull();
        _logger = logger.NotNull();
    }

    public IKeyStore<T>? InnerHandler { get; set; }

    public async Task<Option> Append(string key, T value, ScopeContext context)
    {
        var option = await ProcessLock(key, context);
        if (option.IsError()) return option.LogStatus(context, "Failed to acquire lock for key={key}", [key]);

        if (InnerHandler != null) return await InnerHandler.Append(key, value, context);
        return StatusCode.OK;
    }

    public async Task<Option> Delete(string key, ScopeContext context)
    {
        if (InnerHandler != null) return await InnerHandler.Delete(key, context);
        return StatusCode.OK;
    }

    public async Task<Option<T>> Get(string key, ScopeContext context)
    {
        var option = await ProcessLock(key, context);
        if (option.IsError()) return option.LogStatus(context, "Failed to acquire lock for key={key}", [key]).ToOptionStatus<T>();

        if (InnerHandler != null) return await InnerHandler.Get(key, context);
        return StatusCode.OK;
    }

    public async Task<Option<string>> Set(string key, T value, ScopeContext context)
    {
        var option = await ProcessLock(key, context);
        if (option.IsError()) return option.LogStatus(context, "Failed to acquire lock for key={key}", [key]).ToOptionStatus<string>();

        if (InnerHandler != null) await InnerHandler.Set(key, value, context);
        return StatusCode.OK;
    }

    public async Task<Option> AcquireExclusiveLock(string key, ScopeContext context)
    {
        var option = await ProcessLock(key, context);
        return option;
    }

    public async Task<Option> AcquireLock(string key, ScopeContext context)
    {
        var option = await ProcessLock(key, context);
        return option;
    }

    public async Task<Option> ReleaseLock(string key, ScopeContext context)
    {
        string path = _fileSystem.PathBuilder(key);

        var option = await _lockManager.ReleaseLock(path, context);
        if (option.IsError()) return option.LogStatus(context, "Failed to release lock for path={path}", [path]);
        return option;
    }

    private async Task<Option> ProcessLock(string key, ScopeContext context)
    {
        string path = _fileSystem.PathBuilder(key);

        switch (_lockMode)
        {
            case LockMode.Exclusive:
                context.LogDebug("Acquiring exclusive lock path={path}", path);

                var exclusive = await _lockManager.ProcessLock(path, LockMode.Exclusive, context);
                exclusive.LogStatus(context, "Acquired exclusive lock status for path={path}", [path]);
                return exclusive;

            case LockMode.Shared:
                context.LogDebug("Acquiring shared lock path={path}", path);

                var shared = await _lockManager.ProcessLock(path, LockMode.Shared, context);
                shared.LogStatus(context, "Acquiring lock for path={path}", [path]);
                return shared;

            default:
                throw new InvalidOperationException($"Unknown lock mode '{_lockMode}' for path '{path}'");
        }
    }
}
