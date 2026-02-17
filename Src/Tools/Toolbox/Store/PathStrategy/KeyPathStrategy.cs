using System.Text.RegularExpressions;
using Toolbox.Tools;

namespace Toolbox.Store;

/// <summary>
/// Builds and parses simple (non-hashed) key paths. Supports two formats:
///   {basePath}/{key}
///   {basePath}/{key}.{type}.json
/// Keys may include multiple segments separated by '/' and all generated paths are lower-cased.
/// </summary>
public partial class KeyPathStrategy : IKeyPathStrategy
{
    public KeyPathStrategy(string basePath, bool useCache)
    {
        BasePath = basePath.NotEmpty().TrimStart('/').TrimEnd('/');
        UseCache = useCache;
    }

    public string BasePath { get; }
    public bool UseCache { get; }

    public string BuildPath(string key) => $"{BasePath}/{key.NotEmpty()}".ToLowerInvariant();

    public string BuildPath<T>(string key)
    {
        key.NotEmpty();
        var typeName = typeof(T).Name;
        var result = $"{BasePath}/{key}.{typeName}.json".ToLowerInvariant();
        return result;
    }

    public string BuildSearch(string pattern) => pattern switch
    {
        string v when v.StartsWith(BasePath, StringComparison.OrdinalIgnoreCase) => pattern.ToLowerInvariant(),
        _ => $"{BasePath}/{pattern}".ToLowerInvariant(),
    };

    public string BuildKeySearch(string key) => $"{BasePath}/{key}/**/*".ToLowerInvariant();
    public string BuildDeleteFolder(string path) => $"{BasePath}/{StorePathTool.GetRootPath(path)}";


    public string ExtractKey(string path)
    {
        path.NotEmpty();
        string relativePath = RemoveBasePath(path).ToLowerInvariant();

        Match typedMatch = KeyWithTypeRegex().Match(relativePath);
        if (typedMatch.Success) return typedMatch.Groups["key"].Value;

        Match keyMatch = KeyOnlyRegex().Match(relativePath);
        if (keyMatch.Success) return keyMatch.Groups["key"].Value;

        throw new ArgumentException($"Invalid format path={path}", nameof(path));
    }

    public (string Key, string? TypeName) GetPathParts(string path)
    {
        path.NotEmpty();
        string relativePath = RemoveBasePath(path).ToLowerInvariant();

        Match typedMatch = KeyWithTypeRegex().Match(relativePath);
        if (typedMatch.Success)
        {
            var typeName = typedMatch.Groups["typeName"].Value;
            return (typedMatch.Groups["key"].Value, typeName);
        }

        Match keyMatch = KeyOnlyRegex().Match(relativePath);
        if (keyMatch.Success) return (keyMatch.Groups["key"].Value, null);

        throw new ArgumentException($"Invalid format path={path}", nameof(path));
    }

    public string RemoveBasePath(string path) => path switch
    {
        null => throw new ArgumentNullException(nameof(path)),
        _ when path.StartsWith(BasePath, StringComparison.OrdinalIgnoreCase) => path[BasePath.Length..].TrimStart('/'),
        _ => path,
    };

    // Matches key-only paths: {key...}
    //   key: one or more segments separated by '/'
    [GeneratedRegex(@"^(?<key>[\w.-]+(?:\/[\w.-]+)*)$", RegexOptions.CultureInvariant)]
    private static partial Regex KeyOnlyRegex();

    // Matches typed paths without a type folder: {key...}.{type}.json
    //   key: one or more segments separated by '/'
    //   type: [\w.-]+
    [GeneratedRegex(@"^(?<key>[\w.-]+(?:\/[\w.-]+)*)\.(?<typeName>[\w.-]+)\.json$", RegexOptions.CultureInvariant)]
    private static partial Regex KeyWithTypeRegex();
}
