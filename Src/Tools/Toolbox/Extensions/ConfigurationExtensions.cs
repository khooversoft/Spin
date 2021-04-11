using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

            string json = Json.Default.Serialize(subject).VerifyNotEmpty("Serialization failed");

            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            MemoryStream stream = new MemoryStream(byteArray);

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            var list = new List<string>();
            dump(config, list);

            return list;

            static void dump(IConfiguration configuration, List<string> list)
            {
                foreach (IConfigurationSection section in configuration.GetChildren())
                {
                    if (section.Value == null)
                    {
                        dump(section, list);
                        continue;
                    }

                    list.Add($"{section.Path}={section.Value}");
                }
            }
        }
    }
}