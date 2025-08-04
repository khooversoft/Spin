using Toolbox.Tools;

namespace Toolbox.Store;

public class ListFileSystem : IListFileSystem
{
    public FileSystemType SystemType { get; init; } = FileSystemType.List;
    public string PathBuilder<T>(string key) => PathBuilder(key, typeof(T).Name);

    public virtual string PathBuilder(string key, string listType)
    {
        DateTime now = DateTime.UtcNow;
        return $"{key}/{now:yyyyMM}/{key}-{now:yyyyMMdd}.{listType}.json";
    }
    public virtual string PathBuilder(string key, string listType, DateTime date)
    {
        return $"{key}/{date:yyyyMM}/{key}-{date:yyyyMMdd-HHmmss}.{listType}.json";
    }

    public string SearchBuilder(string key, string? pattern) => pattern switch
    {
        string p => $"{key.NotEmpty()}/{pattern}",
        _ => key,
    };

    public DateTime ExtractTimeIndex(string path) => PartitionSchemas.ExtractTimeIndex(path);
}


public class ListSecondFileSystem : ListFileSystem
{
    public override string PathBuilder(string key, string listType)
    {
        DateTime now = DateTime.UtcNow;
        return $"{key}/{now:yyyyMM}/{key}-{now:yyyyMMdd-HHmmss}.{listType}.json";
    }

    public override string PathBuilder(string key, string listType, DateTime date)
    {
        return $"{key}/{date:yyyyMM}/{key}-{date:yyyyMMdd-HHmmss}.{listType}.json";
    }
}