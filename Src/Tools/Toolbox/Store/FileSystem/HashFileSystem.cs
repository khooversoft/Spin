using Toolbox.Tools;

namespace Toolbox.Store;

public class HashFileSystem : IFileSystem
{
    public FileSystemType SystemType { get; init; } = FileSystemType.Hash;

    public string PathBuilder<T>(string key) => PathBuilder(key, typeof(T).Name);

    public string PathBuilder(string key, string listType)
    {
        var path = $"{key}.{listType}.json";
        var hashPath = PathTool.CreateHashPath(path);

        var result = $"{hashPath}/{path}";
        return result;
    }
}
