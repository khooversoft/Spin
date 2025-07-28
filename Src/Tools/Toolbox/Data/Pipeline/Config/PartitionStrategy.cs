using Toolbox.Tools;

namespace Toolbox.Data;


/// File partitioning schemas
///   File = {h1}/{h2}/{key}.{typeName}.json
///   FileSearch = ?"
///   
///   List = {key}/yyyyMM/{key}-yyyyMMdd.{typeName}.json
///   ListSearch = {key}/?"
///   
/// timeIndex = "yyyyMM"
/// Day timeIndex = "yyyyMMdd"
/// 


public static class PartitionSchemas
{
    public static string ScalarPath<T>(string key)
    {
        key.NotEmpty();

        var path = $"{key}.{typeof(T).Name}.json";
        var hashPath = PathTool.CreateHashPath(path);

        var result = $"{hashPath}/{path}";
        return result;
    }

    public static string ScalarSearch(string _, string pattern) => $"*/*/{pattern.NotEmpty()}";

    public static string ListPath<T>(string key)
    {
        key.NotEmpty();
        DateTime now = DateTime.UtcNow;

        var path = $"{key}/{now:yyyyMM}/{key}-{now:yyyyMMdd}.{typeof(T).Name}.json";
        return path;
    }

    public static string ListSearch(string key, string pattern)
    {
        key.NotEmpty();

        var path = $"{key}/{pattern}";
        return path;
    }
}
