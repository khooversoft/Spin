//using Toolbox.Extensions;

//namespace Toolbox.Store;

//public class KeySystemBase
//{
//    private readonly string _pathPrefix;

//    public KeySystemBase(string? basePath, KeySystemType systemType)
//    {
//        BasePath = basePath;
//        SystemType = systemType;

//        _pathPrefix = basePath switch
//        {
//            null => string.Empty,
//            _ => $"{basePath}".ToLowerInvariant()
//        };
//    }

//    public string? BasePath { get; }
//    public KeySystemType SystemType { get; }

//    public virtual string BuildKeySearch(string key) => $"{_pathPrefix}/{key}/**/*".ToLowerInvariant();

//    public virtual string BuildSearch(string pattern) => $"{_pathPrefix}/{pattern}".ToLowerInvariant();

//    public string GetPathPrefix() => _pathPrefix;
//    public string AddPathPrefix(string path) => $"{_pathPrefix}/{path}".TrimEnd('/').ToLowerInvariant();

//    public string RemovePathPrefix(string path) => path switch
//    {
//        null => throw new ArgumentNullException(nameof(path)),
//        _ when _pathPrefix.IsEmpty() => path,
//        _ when path.StartsWith(_pathPrefix, StringComparison.OrdinalIgnoreCase) => path[_pathPrefix.Length..].TrimStart('/'),
//        _ => path,
//    };

//    public virtual string BuildDeleteFolder(string path) => $"{_pathPrefix}/{StorePathTool.GetRootPath(path)}";
//}
