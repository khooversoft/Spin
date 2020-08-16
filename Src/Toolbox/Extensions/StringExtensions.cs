using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Toolbox.Tools;

namespace Toolbox
{
    public static class StringExtensions
    {
        /// <summary>
        /// Is string null or just white spaces
        /// </summary>
        /// <param name="subject">subject</param>
        /// <returns>true or false</returns>
        public static bool IsEmpty(this string? subject) => string.IsNullOrWhiteSpace(subject);

        /// <summary>
        /// Convert to null if string is "empty"
        /// </summary>
        /// <param name="subject">subject to test</param>
        /// <returns>null or subject</returns>
        public static string? ToNullIfEmpty(this string? subject) => string.IsNullOrWhiteSpace(subject) ? null : subject;

        /// <summary>
        /// Append parameters to string
        /// </summary>
        /// <param name="subject">subject</param>
        /// <param name="parameters">parameters</param>
        /// <returns>parameter, appended</returns>
        public static string? WithParameters(this string? subject, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            if (subject.IsEmpty()) return subject;
            parameters.VerifyNotNull(nameof(parameters));

            return subject + " (" + string.Join(", ", parameters.Select(x => $"{x.Key}={x.Value}")) + ")";
        }

        /// <summary>
        /// Convert string to guid
        /// </summary>
        /// <param name="self">string to convert, empty string will return empty guid</param>
        /// <returns>guid or empty guid</returns>
        public static Guid ToGuid(this string self)
        {
            if (self.IsEmpty())
            {
                return Guid.Empty;
            }

            using (var md5 = MD5.Create())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(self));
                return new Guid(data);
            }
        }
    }
}
