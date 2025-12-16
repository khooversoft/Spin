using Toolbox.Extensions;

namespace Toolbox.Store;

public record KeySystemBase
{
    private readonly string _pathPrefix;

    public KeySystemBase(string? basePath, KeySystemType systemType)
    {
        BasePath = basePath;
        SystemType = systemType;

        _pathPrefix = basePath switch
        {
            null => string.Empty,
            _ => $"{basePath}".ToLowerInvariant()
        };
    }

    public string? BasePath { get; }
    public KeySystemType SystemType { get; }

    public string BuildSearch(string? key = null, string? pattern = null)
    {
        string path = (key, pattern) switch
        {
            (null, null) => $"{_pathPrefix}/**/*",
            (null, _) => $"{_pathPrefix}/{pattern}",
            (_, null) => $"{_pathPrefix}/{key}/**/*",
            _ => $"{_pathPrefix}/{key}/{pattern}"
        };

        return path.ToLowerInvariant();
    }

    public string GetPathPrefix() => _pathPrefix;
    public string AddPathPrefix(string path) => $"{_pathPrefix}/{path}".TrimEnd('/').ToLowerInvariant();

    public string RemovePathPrefix(string path) => path switch
    {
        null => throw new ArgumentNullException(nameof(path)),
        _ when _pathPrefix.IsEmpty() => path,
        _ when path.StartsWith(_pathPrefix, StringComparison.OrdinalIgnoreCase) => path[_pathPrefix.Length..].TrimStart('/'),
        _ => path,

    };

    public string BuildDeleteFolder(string path)
    {
        var trimmed = path.LastIndexOf('/') switch
        {
            -1 => path,
            var idx when idx == path.Length => path,
            var idx when path[idx + 1] == '*' => path[..idx],
            _ => path,
        };

        return $"{_pathPrefix}/{trimmed}/*".ToLowerInvariant();
    }
}
