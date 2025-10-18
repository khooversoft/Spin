using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

// Key index: {typeName}/{key}.{typeName}.json

public class KeyFileSystem<T> : IFileSystem<T>
{
    private readonly FileSystemConfig<T> _config;

    public KeyFileSystem() { }
    public KeyFileSystem(FileSystemConfig<T> config) => _config = config;

    public string? BasePath => _config.BasePath;
    public string Serialize(T subject) => _config.Serialize(subject);
    public T? Deserialize(string data) => _config.Deserialize(data);
    public FileSystemType SystemType { get; } = FileSystemType.Key;

    public string PathBuilder(string key) => PathBuilder(key, typeof(T).Name);

    public string PathBuilder(string key, string listType)
    {
        key.NotEmpty();
        listType.NotEmpty();

        var path = (typeof(T) == typeof(DataETag)) switch
        {
            true => $"{this.CreatePathPrefix()}{listType}/{key}",
            false => $"{this.CreatePathPrefix()}{listType}/{key}.{listType}.json"
        };

        return path.ToLowerInvariant();
    }

    public string BuildSearch(string? key = null, string? pattern = null)
    {
        string path = (key, pattern) switch
        {
            (null, null) => $"{this.CreatePathPrefix()}**/*",
            (null, _) => $"{this.CreatePathPrefix()}{pattern}",
            (_, null) => $"{this.CreatePathPrefix()}{key}/**/*",
            _ => $"{this.CreatePathPrefix()}{key}/{pattern}"
        };

        return path.ToLowerInvariant();
    }
}
