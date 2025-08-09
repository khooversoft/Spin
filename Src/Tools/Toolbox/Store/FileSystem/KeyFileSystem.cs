namespace Toolbox.Store;

// Key index: {typeName}/{key}.{typeName}.json

public class KeyFileSystem<T> : IFileSystem<T>
{
    public KeyFileSystem() { }
    public KeyFileSystem(string? basePath) => BasePath = basePath;

    public string? BasePath { get; init; } = null!;
    public FileSystemType SystemType { get; } = FileSystemType.Key;
    public string PathBuilder(string key) => PathBuilder(key, typeof(T).Name);
    public string PathBuilder(string key, string listType) => $"{this.CreatePathPrefix()}{listType}/{key}.{listType}.json";

    public string BuildSearch(string? key = null, string? pattern = null) => (key, pattern) switch
    {
        (null, null) => $"{this.CreatePathPrefix()}**/*",
        (null, _) => $"{this.CreatePathPrefix()}{pattern}",
        (_, null) => $"{this.CreatePathPrefix()}{key}/**/*",
        _ => $"{this.CreatePathPrefix()}{key}/{pattern}"
    };
}
