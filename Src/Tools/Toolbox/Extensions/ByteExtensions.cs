using System;
using System.Collections.Generic;
using System.Linq;
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
        public static byte[] ToBytes(this string subject)
        {
            if (subject == null) return new byte[0];

            return Encoding.UTF8.GetBytes(subject);
        }

        /// <summary>
        /// COnvert bytes to string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string? BytesToString(this byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;

            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Convert to Json
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="subject">subject</param>
        /// <returns>json</returns>
        public static string ToJson<T>(this T subject) => Json.Default.Serialize(subject);

        /// <summary>
        /// Convert object to bytes
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="subject">object</param>
        /// <returns>bytes</returns>
        public static byte[] ToBytes<T>(this T subject) where T : class
        {
            if (subject == null) return new byte[0];

            string json = subject.ToJson();
            return json.ToBytes();
        }

        /// <summary>
        /// Covert bytes to object (deserialize)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subject"></param>
        /// <returns></returns>
        public static T? ToObject<T>(this byte[] subject)
        {
            string? json = subject.BytesToString();
            if (json == null) return default;

            return Json.Default.Deserialize<T>(json);
        }
    }
}
