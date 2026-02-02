using System.Security.Cryptography;
using System.Text;

namespace Toolbox.Extensions;

public static class ByteExtensions
{
    /// <summary>
    /// Convert string to bytes
    /// </summary>
    /// <param name="subject"></param>
    /// <returns>byte array</returns>
    public static byte[] ToBytes(this string? subject) =>
        subject is null ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(subject);

    /// <summary>
    /// Convert bytes to string
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string BytesToString(this IEnumerable<byte>? bytes)
    {
        ReadOnlySpan<byte> span = bytes.AsReadOnlySpan();
        return Encoding.UTF8.GetString(span);
    }

    /// <summary>
    /// Convert bytes to string
    /// </summary>
    /// <param name="span"></param>
    /// <returns></returns>
    public static string BytesToString(this ReadOnlySpan<byte> span) =>
        span.IsEmpty ? string.Empty : Encoding.UTF8.GetString(span);

    /// <summary>
    /// Bytes to hex string
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string ToHex(this IEnumerable<byte>? bytes)
    {
        ReadOnlySpan<byte> span = bytes.AsReadOnlySpan();
        return Convert.ToHexString(span);
    }

    /// <summary>
    /// Convert object to bytes
    /// </summary>
    /// <typeparam name="T">type</typeparam>
    /// <param name="subject">object</param>
    /// <returns>bytes</returns>
    public static byte[] ToBytes<T>(this T subject) where T : class
    {
        if (subject == null) return Array.Empty<byte>();

        return subject.ToJson().ToBytes();
    }

    /// <summary>
    /// Calculate hash of bytes
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static byte[] ToHash(this IEnumerable<byte>? bytes)
    {
        ReadOnlySpan<byte> span = bytes.AsReadOnlySpan();
        return SHA256.HashData(span);
    }

    public static byte[] ToMD5Hash(this IEnumerable<byte>? bytes)
    {
        ReadOnlySpan<byte> span = bytes.AsReadOnlySpan();
        return MD5.HashData(span);
    }

    /// <summary>
    /// Calculate hash of bytes and return string of hex values
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string ToHexHash(this IEnumerable<byte>? subject)
    {
        ReadOnlySpan<byte> span = subject.AsReadOnlySpan();
        return Convert.ToHexString(SHA256.HashData(span));
    }
}
