namespace Toolbox.Store;

public class KeyFileSystem : IFileSystem
{
    public FileSystemType SystemType { get; init; } = FileSystemType.Key;
    public string PathBuilder<T>(string key) => PathBuilder(key, typeof(T).Name);
    public string PathBuilder(string key, string listType) => $"{listType}/{key}.{listType}.json";
}
