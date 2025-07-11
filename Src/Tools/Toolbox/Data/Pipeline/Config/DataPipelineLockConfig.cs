using System.Collections.Concurrent;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public enum LockMode
{
    Shared,
    Exclusive,
}

public record LockPathConfig
{
    public PathDetail PathDetail { get; init; } = null!;
    public LockMode LockMode { get; init; }
}

public class DataPipelineLockConfig
{
    private ConcurrentDictionary<string, LockPathConfig> _lockConfig = new(StringComparer.OrdinalIgnoreCase);

    public TimeSpan AcquireLockDuration { get; set; } = TimeSpan.FromSeconds(60);

    public void Add<T>(LockMode lockMode, string pipelineName, string key) => Add(pipelineName, typeof(T).Name, key, lockMode);

    public void Add(string pipelineName, string typeName, string key, LockMode lockMode)
    {
        var result = new LockPathConfig
        {
            PathDetail = new PathDetail
            {
                PipelineName = pipelineName.NotEmpty(),
                TypeName = typeName.NotEmpty(),
                Key = key.NotEmpty(),
            },
            LockMode = lockMode.Assert(x => x.IsEnumValid(), "Invalid lock mode"),
        };

        _lockConfig[result.PathDetail.GetKey()] = result;
    }

    public Option<LockMode> GetLockMode(PathDetail lookForPathDetail, ScopeContext context)
    {
        if (_lockConfig.TryGetValue(lookForPathDetail.GetKey(), out LockPathConfig? found))
        {
            context.LogDebug("Found lock mode for pathDetail={pathDetail}: lockMode={lockMode}", lookForPathDetail, found.LockMode);
            return found.LockMode;
        }

        foreach (var item in _lockConfig.Values)
        {
            if (lookForPathDetail.Like(item.PathDetail))
            {
                context.LogDebug("Found lock mode for pathDetail={pathDetail}: lockMode={lockMode}", lookForPathDetail, item.LockMode);
                return item.LockMode;
            }
        }

        context.LogDebug("Lock not found for pathDetail={pathDetail}", lookForPathDetail);
        return StatusCode.NotFound;
    }
}
