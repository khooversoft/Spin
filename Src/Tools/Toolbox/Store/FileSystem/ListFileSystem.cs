using Toolbox.Tools;

namespace Toolbox.Store;

// List index: {key}/yyyyMM/{key}-yyyyMMdd.{typeName}.json
// ListSecond index: {key}/yyyyMM/{key}-yyyyMMdd.{typeName}.json

public class ListFileSystem<T> : IListFileSystem<T>
{
    public ListFileSystem() { }
    public ListFileSystem(string? basePath) => BasePath = basePath;

    public string? BasePath { get; } = null!;
    public FileSystemType SystemType { get; } = FileSystemType.List;

    public virtual string PathBuilder(string key)
    {
        DateTime now = DateTime.UtcNow;
        return $"{this.CreatePathPrefix()}{key}/{now:yyyyMM}/{key}-{now:yyyyMMdd}.{typeof(T).Name}.json";
    }

    public virtual string PathBuilder(string key, DateTime date)
    {
        return $"{this.CreatePathPrefix()}{key}/{date:yyyyMM}/{key}-{date:yyyyMMdd-HHmmss}.{typeof(T).Name}.json";
    }

    public string BuildSearch(string? key = null, string? pattern = null) => (key, pattern) switch
    {
        (null, null) => $"{this.CreatePathPrefix()}**/*",
        (null, _) => $"{this.CreatePathPrefix()}{pattern}",
        (_, null) => $"{this.CreatePathPrefix()}{key}/**/*",
        _ => $"{this.CreatePathPrefix()}{key}/{pattern}"
    };

    public DateTime ExtractTimeIndex(string path) => PartitionSchemas.ExtractTimeIndex(path);
}


public class ListSecondFileSystem<T> : ListFileSystem<T>
{
    public ListSecondFileSystem() { }
    public ListSecondFileSystem(string basePath) : base(basePath) { }

    public override string PathBuilder(string key)
    {
        DateTime now = DateTime.UtcNow;
        return $"{this.CreatePathPrefix()}{key}/{now:yyyyMM}/{key}-{now:yyyyMMdd-HHmmss}.{typeof(T).Name}.json";
    }

    public override string PathBuilder(string key, DateTime date)
    {
        return $"{this.CreatePathPrefix()}{key}/{date:yyyyMM}/{key}-{date:yyyyMMdd-HHmmss}.{typeof(T).Name}.json";
    }
}
