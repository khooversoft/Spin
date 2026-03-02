using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.FileSystemGlobbing;
using Toolbox.Tools;

namespace Toolbox.Extensions;

public static class StringToolExtensions
{
    /// <summary>
    /// Remove duplicate character from string
    /// </summary>
    /// <param name="input"></param>
    /// <param name="removeDuplicateChr"></param>
    /// <returns></returns>
    public static string? RemoveDuplicates(this string? input, char removeDuplicateChr)
    {
        if (input.IsEmpty()) return input;

        ReadOnlySpan<char> inputSpan = input.AsSpan();
        Span<char> outputSpan = stackalloc char[inputSpan.Length];
        int outputIndex = 0;
        int i = 0;
        outputSpan[outputIndex++] = inputSpan[i++];

        for (; i < inputSpan.Length; i++)
        {
            if (inputSpan[i] == removeDuplicateChr && inputSpan[i - 1] == removeDuplicateChr) continue;

            outputSpan[outputIndex++] = inputSpan[i];
        }

        return outputSpan.Slice(0, outputIndex).ToString();
    }

    /// <summary>
    /// Remove trailing characters like '/'
    /// </summary>
    /// <param name="input"></param>
    /// <param name="removeTrailingChr"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(input))]
    public static string? RemoveTrailing(this string? input, char removeTrailingChr)
    {
        if (input.IsEmpty()) return input;

        input = input.EndsWith(removeTrailingChr) ? input[0..(input.Length - 1)] : input;
        return input;
    }

    /// <summary>
    /// Matches input string against a wildcard pattern with case-insensitive comparison.
    /// Supports '*' (matches zero or more characters) and '?' (matches exactly one character).
    /// All other characters are matched literally.
    /// </summary>
    /// <param name="input">The string to match against the pattern. Returns false if null or empty.</param>
    /// <param name="pattern">The wildcard pattern to match. Returns false if null or empty.</param>
    /// <param name="useContains">If true and pattern contains no wildcards, uses Contains instead of Equals for matching.</param>
    /// <returns>True if the input matches the pattern; otherwise, false.</returns>
    public static bool Like(this string? input, string? pattern, bool useContains = false)
    {
        if (input.IsEmpty() || pattern.IsEmpty()) return false;

        if (!pattern.Any(x => x == '*' || x == '?')) return useContains switch
        {
            false => StringComparer.OrdinalIgnoreCase.Equals(input, pattern),
            true => input.Contains(pattern, StringComparison.OrdinalIgnoreCase),
        };

        var builder = new StringBuilder("^");

        foreach (char c in pattern)
        {
            switch (c)
            {
                case '*': builder.Append(".*"); break;
                case '?': builder.Append('.'); break;
                default: builder.Append(Regex.Escape(c.ToString())); break;
            }
        }

        builder.Append('$');

        bool result = Regex.IsMatch(input, builder.ToString(), RegexOptions.IgnoreCase);
        return result;
    }

    /// <summary>
    /// Match using file globbing support
    /// </summary>
    /// <param name="file"></param>
    /// <param name="patterns"></param>
    /// <returns></returns>
    public static bool Match(this string file, params string[] patterns)
    {
        file.NotEmpty();
        if (patterns.Length == 0) return false;

        Matcher matcher = new();
        patterns.ForEach(x => matcher.AddInclude(x));

        var matchResult = matcher.Match(file, "/");
        return matchResult.HasMatches;
    }

    /// <summary>
    /// Upper case the first letter
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(input))]
    public static string? ToUpperFirstLetter(this string? input)
    {
        if (input.IsEmpty()) return input;

        return input.Length switch
        {
            1 => input.ToUpper(),
            _ => char.ToUpper(input[0]) + input[1..],
        };
    }

    /// <summary>
    /// Test if string has wildcard characters '*' or '?'
    /// </summary>
    public static bool HasWildCard(this string? value) => value switch
    {
        null => false,
        string v when v.IsEmpty() => false,
        string v when v.IndexOfAny(['*', '?'], 0) >= 0 => true,
        _ => false,
    };
}
