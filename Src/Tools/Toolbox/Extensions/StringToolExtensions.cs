using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

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
    /// Match input against wilcard pattern
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool IsMatch(this string? input, string? pattern)
    {
        if (input.IsEmpty() || pattern.IsEmpty()) return false;

        if (!pattern.Any(x => x == '*' || x == '?')) return StringComparer.OrdinalIgnoreCase.Equals(input, pattern);

        var builder = new StringBuilder("^");

        foreach (Char c in pattern)
        {
            switch (c)
            {
                case '*': builder.Append(".*"); break;
                case '?': builder.Append('.'); break;
                default: builder.Append("[" + c + "]"); break;
            }
        }

        builder.Append('$');

        bool result = new Regex(builder.ToString(), RegexOptions.IgnoreCase).IsMatch(input);
        return result;
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
}
