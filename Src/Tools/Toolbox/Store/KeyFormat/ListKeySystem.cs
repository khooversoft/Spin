using Toolbox.Tools;

namespace Toolbox.Store;


public class ListKeySystem<T> : KeySystemBase
{
    public ListKeySystem(string basePath)
        : base(basePath, KeySystemType.List)
    {
    }

    public string PathBuilder(string key)
    {
        key.NotEmpty();
        DateTime now = DateTime.UtcNow;
        var result = $"{GetPathPrefix()}/{key}/{now:yyyyMM}/{key}-{now:yyyyMMdd-HH}.{typeof(T).Name}.json";
        return result.ToLowerInvariant();
    }

    public DateTime ExtractTimeIndex(string path) => PartitionSchemas.ExtractTimeIndex(path);
}
