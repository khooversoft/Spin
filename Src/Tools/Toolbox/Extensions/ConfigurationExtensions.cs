using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Toolbox.Tools;

namespace Toolbox.Extensions;

public static class ConfigurationExtensions
{
    public static T Bind<T>(this IConfiguration configuration) where T : new()
    {
        configuration.NotNull();

        var option = new T();
        configuration.Bind(option, x => x.BindNonPublicProperties = true);

        return option;
    }

    public static IConfiguration Bind<T>(this IConfiguration configuration, out T value) where T : new()
    {
        configuration.NotNull();

        value = configuration.Bind<T>();
        return configuration;
    }

    /// <summary>
    /// Uses function to recursive build configuration settings (.Net Core Configuration) from a class that can have sub-classes
    /// </summary>
    /// <typeparam name="T">type of class</typeparam>
    /// <param name="subject">instance of class</param>
    /// <returns>list or configuration settings "propertyName=value"</returns>
    public static IReadOnlyList<KeyValuePair<string, string>> GetConfigurationValues<T>(this T subject) where T : class
    {
        subject.NotNull();

        string json = JsonSerializer.Serialize(subject).NotEmpty(name: "Serialization failed");

        byte[] byteArray = Encoding.UTF8.GetBytes(json);
        using MemoryStream stream = new MemoryStream(byteArray);

        IConfiguration config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        return config
            .AsEnumerable()
            .Where(x => x.Value != null)
            .OrderBy(x => x.Key)
            .ToArray();
    }

    /// <summary>
    /// Convert properties to configuration
    /// </summary>
    /// <param name="lines">lines</param>
    /// <returns>configuration</returns>
    public static IConfiguration ToConfiguration(this IEnumerable<string> lines) => new ConfigurationBuilder()
        .AddCommandLine(lines
            .Select(x => x.ToKeyValuePair())
            .Select(x => $"{x.Key}={x.Value}")
            .ToArray())
        .Build();
}
