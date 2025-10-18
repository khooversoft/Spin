using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

// Hash index: [{basePath}/]{hash}/{hash}/{key}.{typeName}.json

public class HashFileSystem<T> : IFileSystem<T>
{
    private readonly FileSystemConfig<T> _config;

    public HashFileSystem() { }
    public HashFileSystem(FileSystemConfig<T> config) => _config = config;

    public string? BasePath => _config.BasePath;
    public string Serialize(T subject) => _config.Serialize(subject);
    public T? Deserialize(string data) => _config.Deserialize(data);
    public FileSystemType SystemType { get; } = FileSystemType.Hash;
    public string PathBuilder(string key) => PathBuilder(key, typeof(T).Name);

    public string PathBuilder(string key, string listType)
    {
        var path = (typeof(T) == typeof(DataETag)) switch
        {
            true => key,
            false => $"{key}.{listType}.json"
        };

        var hashPath = PathTool.CreateHashPath(path);

        var result = $"{this.CreatePathPrefix()}{hashPath}/{path}";
        return result.ToLowerInvariant();
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
}
