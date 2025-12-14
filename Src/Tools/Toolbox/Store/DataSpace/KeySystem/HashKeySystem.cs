using Toolbox.Tools;

namespace Toolbox.Store;

public record HashKeySystem : KeySystemBase, IKeySystem
{
    public HashKeySystem(string basePath) : base(basePath, KeySystemType.Hash) { }

    public string PathBuilder(string key)
    {
        key.NotEmpty();
        var hashPath = PathTool.CreateHashPath(key);
        var result = $"{this.CreatePathPrefix()}/{hashPath}/{key}";

        return result.ToLowerInvariant();
    }

    public string PathBuilder<T>(string key)
    {
        key.NotEmpty();

        var keyPath = $"{key}.{typeof(T).Name}.json";
        var hashPath = PathTool.CreateHashPath(keyPath);
        var result = $"{this.CreatePathPrefix()}/{hashPath}/{keyPath}";
        return result;
    }
}
