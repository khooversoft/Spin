using System.Collections.Concurrent;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Data;

public enum LockMode
{
    Shared,
    Exclusive,
}

public record LockPathConfig
{
    public string Path { get; init; } = null!;
    public LockMode LockMode { get; init; }
}

public class DataPipelineLockConfig
{
    private ConcurrentDictionary<string, LockPathConfig> _lockConfig = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, LockPathConfig> Paths => _lockConfig;

    public TimeSpan AcquireLockDuration { get; set; } = TimeSpan.FromSeconds(60);

    public void Add(string path, LockMode lockMode) => _lockConfig[path] = new LockPathConfig
    {
        Path = path.NotEmpty(),
        LockMode = lockMode.Assert(x => x.IsEnumValid(), "Invalid lock mode"),
    };
}
