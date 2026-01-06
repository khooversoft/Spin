using System.Text.RegularExpressions;

namespace Toolbox.Tools;

/// <summary>
/// Provides functionality to match file names against glob patterns using simplified wildcard syntax.
/// </summary>
/// <remarks>
/// Glob patterns supported by this class include '*', '**', and '?', which allow flexible matching of
/// file and directory names. The matching is case-insensitive and treats '/' as the directory separator. This class is
/// useful for scenarios such as filtering files in a directory or implementing custom file search logic.
/// 
/// Example:
/// * = all files in folder
/// ** = all files in all subfolders
/// *.json = all .json files in folder
/// **/*.json = all .json files in all subfolders
/// folder1/* = all files in folder1
/// folder1/abc*/** = all files in all subfolders that start with 'abc' in folder1
/// folder1/**/abc?.json = all .json files in all subfolders that start with 'abc' and have one more character in folder1
/// fodler1/*/*/abc*.json = all .json files in folder1's subfolders two levels deep that start with 'abc'
/// 
/// Note: if "***" is used for "**", it is treated as "**" and sets _includeFolders to true.
/// 
/// </remarks>
public class GlobFileMatching
{
    private static readonly Regex _multipleSlashesRegex = new Regex("/{2,}", RegexOptions.Compiled);
    private static readonly Regex _tripleOrMoreStarRegex = new Regex(@"\*{3,}", RegexOptions.Compiled);
    private static readonly Regex _doubleStarRegex = new Regex(@"\*\*", RegexOptions.Compiled);
    private static readonly Regex _multipleStarsInDirectoriesRegex = new Regex(@"\*.*\*", RegexOptions.Compiled);
    private readonly bool _isRecursive;
    private readonly bool _includeFolders;
    private readonly Regex _regex;

    public GlobFileMatching(string pattern)
    {
        pattern.NotEmpty();
        string normalizedPattern = NormalizePath(pattern);

        _includeFolders = _tripleOrMoreStarRegex.IsMatch(normalizedPattern);
        if (_includeFolders)
        {
            normalizedPattern = _tripleOrMoreStarRegex.Replace(normalizedPattern, "**");
        }

        _isRecursive = CalculateIsRecursive(normalizedPattern);

        // Escape special regex characters in the pattern
        string regexPattern = Regex.Escape(normalizedPattern)
            .Replace(@"\*\*/", "(?:.*/)?") // Replace '**/' with '(?:.*/)?' to match zero or more directories
            .Replace(@"\*\*", ".*")       // Replace '**' with '.*' to match any number of directories
            .Replace(@"\*", "[^/]*")      // Replace '*' with '[^/]*' to match any number of characters except '/'
            .Replace(@"\?", "[^/]");      // Replace '?' with '[^/]' to match exactly one character except '/'

        // Add start and end anchors to the regex pattern
        regexPattern = "^" + regexPattern + "$";

        _regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    public bool IsMatch(string fileName)
    {
        string normalizedFileName = NormalizePath(fileName);
        return _regex.IsMatch(normalizedFileName);
    }

    /// <summary>
    /// Using the pattern, determine if the search is recursive.
    /// 
    /// If "**" or more than one "*" is used in a folder level, then the search is recursive.
    /// </summary>
    /// <param name="pattern">Unused; recursion is calculated from the constructor pattern.</param>
    /// <returns></returns>
    public bool IsRecursive => _isRecursive;

    /// <summary>
    /// Indicates whether folder matches should be included (triggered by "***" in the pattern).
    /// </summary>
    public bool IncludeFolders => _includeFolders;

    private static string NormalizePath(string path)
    {
        string normalized = path.NotEmpty().Replace('\\', '/');
        return _multipleSlashesRegex.Replace(normalized, "/");
    }

    private static bool CalculateIsRecursive(string normalizedPattern)
    {
        if (_doubleStarRegex.IsMatch(normalizedPattern))
        {
            return true;
        }

        int lastSlashIndex = normalizedPattern.LastIndexOf('/');
        string directoryPart = lastSlashIndex >= 0 ? normalizedPattern[..lastSlashIndex] : string.Empty;

        return _multipleStarsInDirectoriesRegex.IsMatch(directoryPart);
    }
}
