using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Tools;

namespace Toolbox.Extensions
{
    public static class ConfigurationExtensions
    {
        public static T Bind<T>(this IConfiguration configuration) where T : new()
        {
            configuration.VerifyNotNull(nameof(configuration));

            var option = new T();
            configuration.Bind(option, x => x.BindNonPublicProperties = true);
            return option;
        }

        /// <summary>
        /// Uses function to recursive build configuration settings (.Net Core Configuration) from a class that can have sub-classes
        /// </summary>
        /// <typeparam name="T">type of class</typeparam>
        /// <param name="subject">instance of class</param>
        /// <returns>list or configuration settings "propertyName=value"</returns>
        public static IReadOnlyList<string> GetConfigValues<T>(this T subject) where T : class
        {
            subject.VerifyNotNull(nameof(subject));

            return getProperties(null, subject)
                .ToList();

            static string buildName(string? root, string name) => (root.ToNullIfEmpty() == null ? string.Empty : root + ":") + name;

            // Get properties on object
            static IEnumerable<string> getProperties(string? root, object subject) =>
                subject.GetType().GetProperties()
                .SelectMany(x => getProperty(root, x.Name, x.GetValue(subject)));

            // Get property on object
            static IEnumerable<string> getProperty(string? root, string name, object? subject) => subject switch
            {
                object v when v.GetType() == typeof(string) || !v.GetType().IsClass => new[] { $"{buildName(root, name)}={v!}" },

                IEnumerable<object> v => v.SelectMany((x, i) => getProperties(buildName(root, name) + $":{i}", x)),

                object v => getProperties(buildName(root, name), v),

                _ => Enumerable.Empty<string>(),
            };
        }
    }
}