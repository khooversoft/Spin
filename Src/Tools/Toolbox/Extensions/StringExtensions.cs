using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
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
    public static string ToHashHex(this string subject, bool useMD5 = false) => subject
        .NotEmpty()
        .ToBytes()
        .Func(x => useMD5 ? x.ToMD5Hash() : x.ToHash())
        .ToHex();

    /// <summary>
    /// Ignore case equals
    /// </summary>
    /// <param name="subject">subject to compare</param>
    /// <param name="value">value to compare to</param>
    /// <returns>true or false</returns>
    public static bool EqualsIgnoreCase(this string? subject, string? value) => (subject, value) switch
    {
        (null, null) => true,
        (string v1, string v2) => v1.Equals(v2, StringComparison.OrdinalIgnoreCase),
        _ => false,
    };

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
        null => "**null*",
        string v when v.Length < count * 2 => $"***{v.Length}",
        string v => value[0..count] + "..." + value[^count..],
    };

    /// <summary>
    /// Convert string to base64 encoding
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToBase64(this string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

    /// <summary>
    /// Convert base64 to string
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string FromBase64(this string value) => Encoding.UTF8.GetString(Convert.FromBase64String(value));
}
