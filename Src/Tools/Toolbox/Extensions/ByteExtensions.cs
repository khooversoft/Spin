using System.Security.Cryptography;
using System.Text;
using Toolbox.Tools;

namespace Toolbox.Extensions;

public static class ByteExtensions
{
    /// <summary>
    /// Convert string to bytes
    /// </summary>
    /// <param name="subject"></param>
    /// <returns>byte array</returns>
    public static byte[] ToBytes(this string? subject)
    {
        if (subject == null) return new byte[0];
        return Encoding.UTF8.GetBytes(subject);
    }

    /// <summary>
    /// COnvert bytes to string
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string BytesToString(this IEnumerable<byte> bytes)
    {
        if (bytes == null || bytes.Count() == 0) return string.Empty;
        return Encoding.UTF8.GetString(bytes.ToArray());
    }

    /// <summary>
    /// COnvert bytes to string
    /// </summary>
    /// <param name="span"></param>
    /// <returns></returns>
    public static string BytesToString(this ReadOnlySpan<byte> span)
    {
        if (span == Span<byte>.Empty || span.Length == 0) return string.Empty;
        return Encoding.UTF8.GetString(span);
    }

    /// <summary>
    /// Bytes to hex string
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string ToHex(this IEnumerable<byte> bytes)
    {
        if (bytes == null || bytes.Count() == 0) return string.Empty;

        return bytes
            .Select(x => x.ToString("X02"))
            .Aggregate(string.Empty, (a, x) => a += x);
    }

    /// <summary>
    /// Convert object to bytes
    /// </summary>
    /// <typeparam name="T">type</typeparam>
    /// <param name="subject">object</param>
    /// <returns>bytes</returns>
    public static byte[] ToBytes<T>(this T subject) where T : class
    {
        if (subject == null) return new byte[0];

        return subject.ToJson().ToBytes();
    }

    /// <summary>
    /// Covert bytes to object (deserialize)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static T? ToObject<T>(this byte[] subject)
    {
        subject.NotNull().Assert(x => x.Length > 0, nameof(subject));

        string json = subject.BytesToString();
        return Json.Default.Deserialize<T>(json);
    }

    /// <summary>
    /// Covert bytes to object (deserialize)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static T? ToObject<T>(this ReadOnlySpan<byte> subject)
    {
        subject.Length.Assert(x => x > 0, nameof(subject));
        string json = subject.BytesToString();
        return Json.Default.Deserialize<T>(json);
    }

    /// <summary>
    /// Covert json string to object
    /// </summary>
    /// <typeparam name="T">deserialize to type</typeparam>
    /// <param name="json">json string</param>
    /// <returns>object</returns>
    public static T? ToObject<T>(this string json, bool required = false)
    {
        if (json.IsEmpty()) return default;

        var obj = Json.Default.Deserialize<T>(json);
        return required switch
        {
            false => obj,
            true => obj.NotNull(name: "Deserialize error"),
        };
    }

    /// <summary>
    /// Calculate hash of bytes
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static byte[] ToHash(this IEnumerable<byte> bytes)
    {
        using var scope = SHA256.Create();
        var data = scope.ComputeHash(bytes.NotNull().ToArray());
        return data;
    }

    public static byte[] ToMD5Hash(this IEnumerable<byte> bytes)
    {
        using var scope = MD5.Create();
        var data = scope.ComputeHash(bytes.NotNull().ToArray());
        return data;
    }

    /// <summary>
    /// Calculate hash of bytes and return string of hex values
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string ToHexHash(this IEnumerable<byte> subject) => subject
            .ToHash()
            .ToHex();
}
