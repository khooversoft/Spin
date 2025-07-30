using System.Collections.Concurrent;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class DataPipelineLockConfig
{
    private ConcurrentDictionary<string, PathLock> _lockConfig = new(StringComparer.OrdinalIgnoreCase);

    public TimeSpan AcquireLockDuration { get; set; } = TimeSpan.FromSeconds(60);

    public void Add(string pattern, LockMode lockMode) => _lockConfig[pattern.NotEmpty()] = new PathLock(pattern, lockMode);

    public void Add<T>(LockMode lockMode)
    {
        var pattern = $"*{typeof(T).Name}*";
        _lockConfig[pattern] = new PathLock(pattern, lockMode);
    }

    public Option<LockMode> HasLockMode(string checkPath, ScopeContext context)
    {
        foreach (var item in _lockConfig)
        {
            if (checkPath.Like(item.Value.Pattern))
            {
                context.LogDebug("Found lock mode for path={path}: lockMode={lockMode}", checkPath, item.Value.LockMode);
                return item.Value.LockMode;
            }
        }

        context.LogDebug("Lock not found for checkPath={checkPath}", checkPath);
        return StatusCode.NotFound;
    }

    private record PathLock
    {
        public PathLock(string pattern, LockMode lockMode)
        {
            Pattern = pattern.NotEmpty();
            LockMode = lockMode.Assert(x => x.IsEnumValid(), "Invalid lock mode");
        }

        public string Pattern { get; init; } = null!;
        public LockMode LockMode { get; init; }
    }
}
