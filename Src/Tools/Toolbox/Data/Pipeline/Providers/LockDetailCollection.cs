using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public record LockDetail
{
    public LockDetail(IFileLeasedAccess fileLeasedAccess, bool isExclusive, TimeSpan duration)
    {
        FileLeasedAccess = fileLeasedAccess.NotNull();
        IsExclusive = isExclusive;
        Duration = duration.Assert(x => x.TotalSeconds > 1, x => $"Invalid duration={x}");
        Path = fileLeasedAccess.Path.NotEmpty();
    }

    public IFileLeasedAccess FileLeasedAccess { get; }
    public bool IsExclusive { get; }
    public DateTime AcquiredDate { get; } = DateTime.UtcNow;
    public string Path { get; }
    public TimeSpan Duration { get; }
}


public class LockDetailCollection : IAsyncDisposable
{
    private ConcurrentDictionary<string, LockDetail> _lockMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly IFileStore _fileStore;
    private readonly ILogger<LockDetailCollection> _logger;
    private static TimeSpan _defaultTimeout = TimeSpan.FromSeconds(60);

    public LockDetailCollection(IFileStore fileStore, ILogger<LockDetailCollection> logger)
    {
        _fileStore = fileStore.NotNull();
        _logger = logger.NotNull();
    }

    public LockDetail? Contains(string path)
    {
        if (!_lockMap.TryGetValue(path, out LockDetail? detail)) return null;

        if (!IsValid(detail))
        {
            _logger.LogWarning("Lock for file {File} has expired, removing from lock map", detail.Path);
            _lockMap.TryRemove(path, out _);
            return null;
        }

        return detail;
    }

    public void Set(LockDetail detail) => _lockMap.AddOrUpdate(detail.Path, detail, (key, oldValue) => detail);

    public async ValueTask DisposeAsync()
    {
        var context = _logger.ToScopeContext();
        context.LogDebug("Disposing LockDetailCollection, clearing all locks");

        var release = _lockMap.Values
            .Where(x => IsValid(x))
            .ToArray();

        _lockMap.Clear();

        foreach (var item in release)
        {
            context.LogDebug("Releasing lock for file {File}", item.Path);
            await item.FileLeasedAccess.DisposeAsync().ConfigureAwait(false);
        }
    }

    private bool IsValid(LockDetail detail) => detail.IsExclusive || detail.AcquiredDate + _defaultTimeout > DateTime.UtcNow;
}