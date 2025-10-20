using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class KeyStore<T> : IKeyStore<T>
{
    private readonly IFileStore _fileStore;
    private readonly IFileSystem<T> _fileSystem;
    private readonly ILogger<KeyStore<T>> _logger;
    private readonly LockManager _lockManager;

    public KeyStore(IFileStore fileStore, LockManager lockManager, IFileSystem<T> fileSystem, ILogger<KeyStore<T>> logger)
    {
        _fileStore = fileStore.NotNull();
        _lockManager = lockManager.NotNull();
        _fileSystem = fileSystem.NotNull();
        _logger = logger.NotNull();
    }

    public IKeyStore<T>? InnerHandler { get; set; }

    public async Task<Option> Append(string key, T value, ScopeContext context)
    {
        key.NotEmpty();
        value.NotNull();
        context.With(_logger);

        string path = _fileSystem.PathBuilder(key);
        context.LogDebug("Appending path={path}", path);

        var data = value.ToJson().ToDataETag();
        var detailsOption = await _lockManager.GetReadWriteAccess(path, context).Append(data, context);
        if (detailsOption.IsError())
        {
            context.LogDebug("Fail to append to path={path}", path);
            return detailsOption.ToOptionStatus();
        }

        if (InnerHandler != null) return await InnerHandler.Append(key, value, context);
        return StatusCode.OK;
    }

    public async Task<Option> Delete(string key, ScopeContext context)
    {
        key.NotEmpty();
        context.With(_logger);

        string path = _fileSystem.PathBuilder(key);
        context.LogDebug("Deleting path={path}", path);

        var deleteOption = await _fileStore.File(path).Delete(context);
        if (deleteOption.IsError())
        {
            context.LogDebug("Fail to delete path={path}", path);
            return deleteOption;
        }

        if (InnerHandler != null) return await InnerHandler.Delete(key, context);
        return StatusCode.OK;
    }

    public async Task<Option<T>> Get(string key, ScopeContext context)
    {
        key.NotEmpty();
        context.With(_logger);

        string path = _fileSystem.PathBuilder(key);
        context.LogDebug("Getting path={path}", path);

        var readOption = await _fileStore.File(path).Get(context);
        if (readOption.IsError())
        {
            if (InnerHandler != null)
            {
                var innerOption = await InnerHandler.Get(key, context);
                if (innerOption.IsOk()) await OnSet(key, innerOption.Return(), context);

                return innerOption;
            }

            context.LogDebug("Fail to read path={path}", path);
            return StatusCode.NotFound;
        }

        var data = readOption.Return();
        if (data.Data.Length == 0)
        {
            context.LogDebug("File is zero length path={path}", path);
            return (StatusCode.NoContent, "File is zero length");
        }

        context.LogDebug("Found path={path}", path);
        T value = data.ToObject<T>();
        return value;
    }

    public async Task<Option<string>> Set(string key, T value, ScopeContext context)
    {
        key.NotEmpty();
        context.With(_logger);

        var setOption = await OnSet(key, value, context);
        if (setOption.IsError()) return setOption;

        if (InnerHandler != null) return await InnerHandler.Set(key, value, context);
        return setOption;
    }

    public async Task<Option> AcquireExclusiveLock(string key, ScopeContext context)
    {
        key.NotEmpty();
        context.With(_logger);

        string path = _fileSystem.PathBuilder(key);
        context.LogDebug("Acquiring exclusive lock path={path}", path);

        var resultOption = await _lockManager.ProcessLock(path, LockMode.Exclusive, context);
        if (resultOption.IsError()) resultOption.LogStatus(context, "Failed to acquire exclusive lock for path={path}", [path]);

        if (InnerHandler != null) return await InnerHandler.AcquireExclusiveLock(key, context);
        return resultOption;
    }

    public async Task<Option> AcquireLock(string key, ScopeContext context)
    {
        key.NotEmpty();
        context.With(_logger);

        string path = _fileSystem.PathBuilder(key);
        context.LogDebug("Acquiring lock path={path}", path);

        var resultOption = await _lockManager.ProcessLock(path, LockMode.Shared, context);
        if (resultOption.IsError()) resultOption.LogStatus(context, "Failed to acquire lock for path={path}", [path]);

        if (InnerHandler != null) return await InnerHandler.AcquireLock(key, context);
        return resultOption;
    }

    public async Task<Option> ReleaseLock(string key, ScopeContext context)
    {
        key.NotEmpty();
        context.With(_logger);

        string path = _fileSystem.PathBuilder(key);
        context.LogDebug("Releasing lock path={path}", path);

        var resultOption = await _lockManager.ReleaseLock(path, context);
        if (resultOption.IsError()) resultOption.LogStatus(context, "Failed to acquire lock for path={path}", [path]);

        if (InnerHandler != null) return await InnerHandler.ReleaseLock(key, context);
        return resultOption;
    }

    private async Task<Option<string>> OnSet(string key, T value, ScopeContext context)
    {
        key.NotEmpty();
        value.NotNull();
        context.With(_logger);

        string path = _fileSystem.PathBuilder(key);
        context.LogDebug("Setting path={path}", path);

        var data = value.ToDataETag();
        var setOption = await _lockManager.GetReadWriteAccess(path, context).Set(data, context);
        setOption.LogStatus(context, "Write path={path}", [path]);
        return setOption;
    }
}
