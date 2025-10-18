using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Store;

// List index: {key}/yyyyMM/{key}-yyyyMMdd.{typeName}.json
// ListSecond index: {key}/yyyyMM/{key}-yyyyMMdd.{typeName}.json

public class ListFileSystem<T> : IListFileSystem<T>
{
    private readonly FileSystemConfig<T> _config;

    public ListFileSystem() { }
    public ListFileSystem(FileSystemConfig<T> config) => _config = config;

    public string? BasePath => _config.BasePath;
    public string Serialize(T subject) => _config.Serialize(subject);
    public T? Deserialize(string data) => _config.Deserialize(data);
    public FileSystemType SystemType { get; } = FileSystemType.List;

    public virtual string PathBuilder(string key)
    {
        DateTime now = DateTime.UtcNow;
        return $"{this.CreatePathPrefix()}{key}/{now:yyyyMM}/{key}-{now:yyyyMMdd}.{typeof(T).Name}.json".ToLowerInvariant();
    }

    public string BuildSearch(string? key = null, string? pattern = null)
    {
        var path = (key, pattern) switch
        {
            (null, null) => $"{this.CreatePathPrefix()}**/*",
            (null, _) => $"{this.CreatePathPrefix()}{pattern}",
            (_, null) => $"{this.CreatePathPrefix()}{key}/**/*",
            _ => $"{this.CreatePathPrefix()}{key}/{pattern}"
        };

        return path.ToLowerInvariant();
    }

    public DateTime ExtractTimeIndex(string path) => PartitionSchemas.ExtractTimeIndex(path);
}


public class ListSecondFileSystem<T> : ListFileSystem<T>
{
    public ListSecondFileSystem() { }
    public ListSecondFileSystem(FileSystemConfig<T> config) : base(config) { }

    public override string PathBuilder(string key)
    {
        DateTime now = DateTime.UtcNow;
        return $"{this.CreatePathPrefix()}{key}/{now:yyyyMM}/{key}-{now:yyyyMMdd-HHmmss}.{typeof(T).Name}.json".ToLowerInvariant();
    }
}
