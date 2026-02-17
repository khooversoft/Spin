//using Toolbox.Tools;

//namespace Toolbox.Store;

//public class KeySystem : KeySystemBase, IKeySystem
//{
//    public KeySystem(string basePath, bool useCache)
//        : base(basePath, KeySystemType.Key)
//    {
//        UseCache = useCache;
//    }

//    public bool UseCache { get; }

//    public string PathBuilder(string key) => $"{this.GetPathPrefix()}/{key.NotEmpty()}".ToLowerInvariant();

//    public string PathBuilder<T>(string key)
//    {
//        key.NotEmpty();
//        var typeName = typeof(T).Name;
//        var result = $"{GetPathPrefix()}/{typeName}/{key}.{typeName}.json".ToLowerInvariant();
//        return result.ToLowerInvariant();
//    }
//}
