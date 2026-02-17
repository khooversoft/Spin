using System.Text.RegularExpressions;
using Toolbox.Tools;

namespace Toolbox.Store;

/// <summary>
/// Builds and parses hashed key paths using two supported formats:
///   {basePath}/{h1}/{h2}/{key}
///   {basePath}/{h1}/{h2}/{key}.{type}.json
/// Keys may contain multiple '/' segments; hash folders (h1/h2) are derived from the key and all generated paths are lower-cased.
/// </summary>
public partial class KeyHashStrategy : IKeyPathStrategy
{
    public KeyHashStrategy(string basePath, bool useCache)
    {
        BasePath = basePath.NotEmpty().TrimStart('/').TrimEnd('/').ToLowerInvariant();
        UseCache = useCache;
    }

    public string BasePath { get; }
    public bool UseCache { get; }

    public string BuildPath(string key)
    {
        key.NotEmpty();
        var hashPath = PathTool.CreateHashPath(key);
        var result = $"{BasePath}/{hashPath}/{key}";

        return result.ToLowerInvariant();
    }

    public string BuildPath<T>(string key)
    {
        key.NotEmpty();
        var hashPath = PathTool.CreateHashPath(key);
        var typeName = typeof(T).Name;
        var result = $"{BasePath}/{hashPath}/{key}.{typeName}.json";
        return result.ToLowerInvariant();
    }

    public string BuildKeySearch(string key) => $"{BasePath}/*/*/{key}/**".ToLowerInvariant();

    public string BuildSearch(string pattern) => pattern switch
    {
        string v when v.StartsWith(BasePath, StringComparison.OrdinalIgnoreCase) => pattern.ToLowerInvariant(),
        _ => $"{BasePath}/*/*/{pattern}".ToLowerInvariant(),
    };

    public string ExtractKey(string path)
    {
        path.NotEmpty();

        string relativePath = RemoveBasePath(path);

        Match typedMatch = KeyWithTypeRegex().Match(relativePath);
        if (typedMatch.Success) return typedMatch.Groups["key"].Value;

        Match keyMatch = KeyOnlyRegex().Match(relativePath);
        if (keyMatch.Success) return keyMatch.Groups["key"].Value;

        throw new ArgumentException($"Invalid format path={path}", nameof(path));
    }

    public string RemoveBasePath(string path) => path switch
    {
        null => throw new ArgumentNullException(nameof(path)),
        _ when path.StartsWith(BasePath, StringComparison.OrdinalIgnoreCase) => path[BasePath.Length..].TrimStart('/'),
        _ => path,
    };

    // Matches hash-prefixed key-only paths: {h1}/{h2}/{key...}
    //   h1,h2: [\w.-]+
    //   key: one or more segments separated by '/'
    [GeneratedRegex(@"^[\w.-]+\/[\w.-]+\/(?<key>[\w.-]+(?:\/[\w.-]+)*)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex KeyOnlyRegex();

    // Matches hash-prefixed typed paths: {h1}/{h2}/{key...}.{type}.json
    //   h1,h2: [\w.-]+
    //   key: one or more segments separated by '/'
    //   type: [\w.-]+
    [GeneratedRegex(@"^[\w.-]+\/[\w.-]+\/(?<key>[\w.-]+(?:\/[\w.-]+)*)\.(?<typeName>[\w.-]+)\.json$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex KeyWithTypeRegex();
}
