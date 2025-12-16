using Toolbox.Tools;

namespace Toolbox.Store;

public record KeySystem : KeySystemBase, IKeySystem
{
    public KeySystem(string basePath) : base(basePath, KeySystemType.Key) { }

    public string PathBuilder(string key) => $"{this.GetPathPrefix()}/{key.NotEmpty()}".ToLowerInvariant();

    public string PathBuilder<T>(string key)
    {
        key.NotEmpty();
        var typeName = typeof(T).Name;
        var result = $"{GetPathPrefix()}/{typeName}/{key}.{typeName}.json".ToLowerInvariant();
        return result.ToLowerInvariant();
    }
}
