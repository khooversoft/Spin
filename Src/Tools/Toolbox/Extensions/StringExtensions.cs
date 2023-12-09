using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Toolbox.Tools;

namespace Toolbox.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Is string null or just white spaces
    /// </summary>
    /// <param name="subject">subject</param>
    /// <returns>true or false</returns>
    public static bool IsEmpty([NotNullWhen(false)] this string? subject) => string.IsNullOrWhiteSpace(subject);

    /// <summary>
    /// Is string null or just white spaces
    /// </summary>
    /// <param name="subject">subject</param>
    /// <returns>true or false</returns>
    public static bool IsNotEmpty([NotNullWhen(true)] this string? subject) => !string.IsNullOrWhiteSpace(subject);

    /// <summary>
    /// Convert to null if string is "empty"
    /// </summary>
    /// <param name="subject">subject to test</param>
    /// <returns>null or subject</returns>
    public static string? ToNullIfEmpty(this string? subject) => string.IsNullOrWhiteSpace(subject) ? null : subject;

    /// <summary>
    /// Convert string to guid
    /// </summary>
    /// <param name="self">string to convert, empty string will return empty guid</param>
    /// <returns>guid or empty guid</returns>
    public static Guid ToGuid(this string self)
    {
        if (self.IsEmpty()) return Guid.Empty;

        using (var md5 = MD5.Create())
        {
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(self));
            return new Guid(data);
        }
    }

    /// <summary>
    /// Join vector(s) to string with string delimiter
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="delimiter">delimiter to use in join</param>
    /// <returns>result</returns>
    public static string Join(this IEnumerable<string?> values, string delimiter = "") => string.Join(delimiter, values.Where(x => x != null));

    /// <summary>
    /// Join vector(s) to string with character delimiter
    /// </summary>
    /// <param name="values"></param>
    /// <param name="delimiter"></param>
    /// <returns></returns>
    public static string Join(this IEnumerable<string?> values, char delimiter) => string.Join(delimiter, values.Where(x => x != null));


    /// <summary>
    /// Return string's hash in hex numeric form
    /// </summary>
    /// <param name="subject"></param>
    /// <returns>hex values for hash</returns>
    public static string ToHashHex(this string subject) => subject
        .NotEmpty()
        .ToBytes()
        .ToHash()
        .ToHex();

    /// <summary>
    /// Ignore case equals
    /// </summary>
    /// <param name="subject">subject to compare</param>
    /// <param name="value">value to compare to</param>
    /// <returns>true or false</returns>
    public static bool EqualsIgnoreCase(this string subject, string value) => subject.Equals(value, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Compute hash for multiple objects, using string representations
    /// </summary>
    /// <param name="values">values</param>
    /// <returns>hash bytes</returns>
    public static byte[] ComputeHash(this IEnumerable<object?> values)
    {
        values.NotNull();

        var ms = new MemoryStream();

        values.Select(x => x switch
        {
            null => null,
            string v => v.ToBytes(),
            byte[] v => v,

            var v => throw new InvalidDataException($"Not supported type={(v?.GetType()?.Name ?? "<null>")}"),
        })
        .ForEach(x => ms.Write(x));

        ms.Seek(0, SeekOrigin.Begin);
        return MD5.Create().ComputeHash(ms);
    }

    /// <summary>
    /// Truncate a string based on max length value
    /// </summary>
    /// <param name="subject">string or null</param>
    /// <param name="maxLength">max length allowed, must be positive or defaults to 0</param>
    /// <returns>truncate string if required</returns>
    [DebuggerStepThrough]
    [return: NotNullIfNotNull(nameof(subject))]
    public static string? Truncate(this string? subject, int maxLength) => subject switch
    {
        null => null,
        string v => v[..Math.Min(v.Length, Math.Max(maxLength, 0))],
    };

    /// <summary>
    /// Primary used for logging secrets, just the first and last part of the secret is returned
    /// if requirements are meet.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static string GetSecretThumbprint(this string value, int count = 3) => value switch
    {
        null => "***",
        string v when v.Length < count * 2 => "***",
        string v => value[0..count] + "..." + value[^count..],
    };

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
}
