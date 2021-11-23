using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Toolbox.Tools;

namespace Toolbox.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Is string null or just white spaces
        /// </summary>
        /// <param name="subject">subject</param>
        /// <returns>true or false</returns>
        public static bool IsEmpty([NotNullWhen(false)] this string? subject) => string.IsNullOrWhiteSpace(subject);

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
        /// Convert property string ex "property1=value1;property2=value2";
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="propertyDelimiter"></param>
        /// <param name="valueDelimiter"></param>
        /// <returns></returns>
        public static IReadOnlyDictionary<string, string> ToDictionary(this string subject, string propertyDelimiter = ";", string valueDelimiter = "=")
        {
            if (subject.IsEmpty()) return new Dictionary<string, string>();

            KeyValuePair<string, string> GetKeyValue(string property)
            {
                int index = property.IndexOf(valueDelimiter).VerifyAssert(x => x >= 0, $"Syntax error, no {valueDelimiter} for {property}");
                return new KeyValuePair<string, string>(property[0..index], property[(index + 1)..^0]);
            }

            return subject
                .Split(propertyDelimiter, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => GetKeyValue(x))
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public static string Join(this IEnumerable<string> values, string delimiter = "/") => string.Join(delimiter, values);

        public static string ToHashHex(this string subject) => subject
            .VerifyNotEmpty(nameof(subject))
            .ToBytes()
            .ToHash()
            .ToHex();
    }
}
