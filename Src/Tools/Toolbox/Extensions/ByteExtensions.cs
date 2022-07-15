using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Extensions
{
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
            if (bytes == null || bytes.Count() == 0) return String.Empty;

            return Encoding.UTF8.GetString(bytes.ToArray());
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
        /// Calculate hash of bytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] ToHash(this IEnumerable<byte> bytes) => MD5.Create().ComputeHash(bytes.ToArray());

        /// <summary>
        /// Convert to Json
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="subject">subject</param>
        /// <returns>json</returns>
        public static string ToJson<T>(this T subject) => Json.Default.Serialize(subject);

        /// <summary>
        /// Convert to Json formatted
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="subject">subject</param>
        /// <returns>json</returns>
        public static string ToJsonFormat<T>(this T subject) => Json.Default.SerializeFormat(subject);

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
            subject
                .NotNull()
                .Assert(x => x.Length > 0, nameof(subject));

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
    }
}
