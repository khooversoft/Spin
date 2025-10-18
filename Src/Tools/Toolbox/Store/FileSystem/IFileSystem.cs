using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Store;

// Hash index: {hash}/{hash}/{key}.{typeName}.json
// Key index: {typeName}/{key}.{typeName}.json
// List index: {key}/yyyyMM/{key}-yyyyMMdd.{typeName}.json
// ListSecond index: {key}/yyyyMM/{key}-yyyyMMdd.{typeName}.json

public enum FileSystemType
{
    Hash,
    Key,
    List
}

public interface IFileSystem<T>
{
    public string? BasePath { get; }
    public FileSystemType SystemType { get; }
    string PathBuilder(string key);
    string BuildSearch(string? key = null, string? pattern = null);
    string Serialize(T subject);
    T? Deserialize(string data);
}

public interface IListFileSystem<T> : IFileSystem<T>
{
    DateTime ExtractTimeIndex(string path);
}

public static class FileSystemTool
{
    public static string CreatePathPrefix<T>(this IFileSystem<T> subject) => subject.BasePath switch
    {
        null => string.Empty,
        _ => $"{subject.BasePath}/".ToLowerInvariant(),
    };
}