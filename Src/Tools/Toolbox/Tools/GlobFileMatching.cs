using System.Text.RegularExpressions;

namespace Toolbox.Tools;

public class GlobFileMatching
{
    private readonly Regex _regex;

    public GlobFileMatching(string pattern)
    {
        // Escape special regex characters in the pattern
        string regexPattern = Regex.Escape(pattern.NotEmpty())
            .Replace(@"\*\*/", "(?:.*/)?") // Replace '**/' with '(?:.*/)?' to match zero or more directories
            .Replace(@"\*\*", ".*")       // Replace '**' with '.*' to match any number of directories
            .Replace(@"\*", "[^/]*")      // Replace '*' with '[^/]*' to match any number of characters except '/'
            .Replace(@"\?", "[^/]");      // Replace '?' with '[^/]' to match exactly one character except '/'

        // Add start and end anchors to the regex pattern
        regexPattern = "^" + regexPattern + "$";

        _regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    public bool IsMatch(string fileName) => _regex.IsMatch(fileName);
}
