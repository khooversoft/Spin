using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Toolbox.Tools;
using Toolbox.Types;

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

    /// <summary>
    /// Convert an object to its configuration properties {key[:key...}}:{value}
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
            .OfType<KeyValuePair<string, string>>()
            .OrderBy(x => x.Key)
            .ToArray();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <returns></returns>
    public static T ToObject<T>(this IEnumerable<KeyValuePair<string, string>> values) where T : new()
    {
        var dict = values.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        T result = new ConfigurationBuilder()
            .AddInMemoryCollection(values!)
            .Build()
            .Bind<T>();

        return result;
    }
}
