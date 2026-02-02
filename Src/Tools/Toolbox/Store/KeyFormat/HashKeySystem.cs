using Toolbox.Tools;

namespace Toolbox.Store;

public class HashKeySystem : KeySystemBase, IKeySystem
{
    public HashKeySystem(string basePath) : base(basePath, KeySystemType.Hash) { }

    public override string BuildKeySearch(string key) => $"{this.GetPathPrefix()}/*/*/{key}/**";
    public override string BuildSearch(string pattern) => $"{this.GetPathPrefix()}/*/*/{pattern}";

    public override string BuildDeleteFolder(string path)
    {
        var rootBpath = StorePathTool.GetRootPath(path);
        return $"{this.GetPathPrefix()}/*/*/{path}";
    }

    public string PathBuilder(string key)
    {
        key.NotEmpty();
        var hashPath = PathTool.CreateHashPath(key);
        var result = $"{this.GetPathPrefix()}/{hashPath}/{key}";

        return result.ToLowerInvariant();
    }

    public string PathBuilder<T>(string key)
    {
        key.NotEmpty();

        var keyPath = $"{key}.{typeof(T).Name}.json";
        var hashPath = PathTool.CreateHashPath(keyPath);
        var result = $"{this.GetPathPrefix()}/{hashPath}/{keyPath}";
        return result;
    }
}
