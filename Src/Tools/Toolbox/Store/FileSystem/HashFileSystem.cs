using Toolbox.Tools;

namespace Toolbox.Store;

// Hash index: [{basePath}/]{hash}/{hash}/{key}.{typeName}.json

public class HashFileSystem<T> : IFileSystem<T>
{
    public HashFileSystem() { }
    public HashFileSystem(string? basePath) => BasePath = basePath;

    public string? BasePath { get; }
    public FileSystemType SystemType { get; } = FileSystemType.Hash;
    public string PathBuilder(string key) => PathBuilder(key, typeof(T).Name);

    public string PathBuilder(string key, string listType)
    {
        var path = $"{key}.{listType}.json";
        var hashPath = PathTool.CreateHashPath(path);

        var result = $"{this.CreatePathPrefix()}{hashPath}/{path}";
        return result;
    }

    public string BuildSearch(string? key = null, string? pattern = null) => (key, pattern) switch
    {
        (null, null) => $"{this.CreatePathPrefix()}**/*",
        (null, _) => $"{this.CreatePathPrefix()}{pattern}",
        (_, null) => $"{this.CreatePathPrefix()}{key}/**/*",
        _ => $"{this.CreatePathPrefix()}{key}/{pattern}"
    };
}
