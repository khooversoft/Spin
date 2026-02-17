using System.Text.RegularExpressions;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Store;

/// <summary>
/// Builds and parses list key paths using the format: each append adds an entry
///   {basePath}/{key}/{singleKey}-{timestamp}-{counter}-{randomHex}.{type}.json
/// "key" may contain multiple '/' segments; all generated paths are lower-cased.
/// "singleKey" converts key's '/' to '_' to identify file as part of the key
/// "seqNumber" is from the LogSequenceNumber "{timestamp}-{counter}-{rnd}"
/// "type" is the type name of T
/// </summary>
public partial class KeyListStrategy<T>
{
    private readonly LogSequenceNumber _logSequenceNumber;

    public KeyListStrategy(string basePath, LogSequenceNumber logSequenceNumber, int maxBlockSizeMB = 8)
    {
        BasePath = basePath.NotEmpty().TrimStart('/').TrimEnd('/');
        _logSequenceNumber = logSequenceNumber.NotNull();
        MaxBlockSizeMB = maxBlockSizeMB;
        MaxBlockSizeBytes = maxBlockSizeMB * 1024 * 1024;
    }

    public string BasePath { get; }
    public int MaxBlockSizeMB { get; }
    public int MaxBlockSizeBytes { get; }

    public string BuildPath(string key)
    {
        key.NotEmpty();
        string seqNumber = _logSequenceNumber.Next();
        string singleKey = PathTool.BuildSingleSegementFromMany(key);
        var result = $"{BasePath}/{key}/{singleKey}-{seqNumber}.{typeof(T).Name}.json";
        return result.ToLowerInvariant();
    }

    public string BuildSearch(string pattern) => $"{BasePath}/{pattern}".ToLowerInvariant();

    public string BuildKeySearch(string key)
    {
        string singleKey = PathTool.BuildSingleSegementFromMany(key);
        return $"{BasePath}/{key}/{singleKey}-*".ToLowerInvariant();
    }

    public string BuildDeleteFolder(string key) => $"{BasePath}/{key}".ToLowerInvariant();

    public string ExtractKey(string path)
    {
        path.NotEmpty();
        string relativePath = RemoveBasePath(path);

        Match typedMatch = KeyWithRegex().Match(relativePath);
        if (typedMatch.Success) return typedMatch.Groups["key"].Value.ToLowerInvariant();

        throw new ArgumentException($"Invalid format path={path}", nameof(path));
    }

    public (string Key, string SeqNumber, string TypeName) GetPathParts(string path)
    {
        path.NotEmpty();
        string relativePath = RemoveBasePath(path).ToLowerInvariant();

        Match typedMatch = KeyWithRegex().Match(relativePath);
        if (typedMatch.Success)
        {
            var seqNumber = typedMatch.Groups["seqNumber"].Value;
            var typeName = typedMatch.Groups["typeName"].Value;
            return (typedMatch.Groups["key"].Value, seqNumber, typeName);
        }

        throw new ArgumentException($"Invalid format path={path}", nameof(path));
    }

    public bool IsValidPath(string path)
    {
        path.NotEmpty();
        string relativePath = RemoveBasePath(path).ToLowerInvariant();

        Match typedMatch = KeyWithRegex().Match(relativePath);
        return typedMatch.Success;
    }

    public string AddBasePath(string path) => path.ToNullIfEmpty() switch
    {
        null => throw new ArgumentNullException(nameof(path)),
        string v when path.StartsWith(BasePath, StringComparison.OrdinalIgnoreCase) => path,
        string v => $"{BasePath}/{v}".ToLowerInvariant(),
    };

    public string RemoveBasePath(string path) => path.ToNullIfEmpty() switch
    {
        null => throw new ArgumentNullException(nameof(path)),
        _ when path.StartsWith(BasePath, StringComparison.OrdinalIgnoreCase) => path[BasePath.Length..].TrimStart('/'),
        _ => path,
    };

    // Matches list paths: {key...}/{singleKey}-{timestamp}-{counter}-{randomHex}.{type}.json
    //   key: one or more segments separated by '/'
    //   singleKey: alphanumeric plus '_'
    //   timestamp: 15 digits
    //   counter: 6 digits
    //   randomHex: 4 alphanumeric characters
    //   typeName: [\w.-]+
    [GeneratedRegex(@"^(?<key>[\w.-]+(?:\/[\w.-]+)*)\/(?<singleKey>[A-Za-z0-9_]+)-(?<seqNumber>\d{15}-\d{6}-[A-Za-z0-9]{4})\.(?<typeName>[\w.-]+)\.json$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex KeyWithRegex();
}
