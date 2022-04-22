using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Property;

namespace Toolbox.Configuration;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddPropertyResolver(this IConfigurationBuilder configurationBuilder, string secretId)
    {
        configurationBuilder.VerifyNotNull(nameof(configurationBuilder));

        configurationBuilder.Add(new PropertyResolverSource(secretId));

        return configurationBuilder;
    }

    public static IConfigurationBuilder AddPropertyResolver(this IConfigurationBuilder configurationBuilder, IEnumerable<KeyValuePair<string, string>> values)
    {
        configurationBuilder.VerifyNotNull(nameof(configurationBuilder));

        configurationBuilder.Add(new PropertyResolverSource(values));

        return configurationBuilder;
    }

    public static IConfigurationBuilder AddPropertyResolver(this IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.VerifyNotNull(nameof(configurationBuilder));

        IConfiguration configuration = configurationBuilder.Build();

        // Add source
        configurationBuilder.AddSource(_ => new ResolverConfigurationProvider(configuration, configuration.BuildResolver()));

        return configurationBuilder;
    }

    public static IConfigurationBuilder AddJsonFile(this IConfigurationBuilder configurationBuilder, string file, JsonFileOption jsonFileOption, bool optional = false)
    {
        configurationBuilder.VerifyNotNull(nameof(configurationBuilder));
        file.VerifyNotEmpty(nameof(file));

        if (jsonFileOption != JsonFileOption.Enhance)
        {
            configurationBuilder.AddJsonFile(file, optional);
            return configurationBuilder;
        }

        IPropertyResolver resolver = configurationBuilder
            .Build()
            .BuildResolver();

        ConfigurationTools.GetJsonFiles(file, resolver)
            .ForEach(x => configurationBuilder.AddJsonFile(x, optional));

        return configurationBuilder;
    }

    public static IConfigurationBuilder AddResourceStream(this IConfigurationBuilder configurationBuilder, Type type, string name)
    {
        configurationBuilder.VerifyNotNull(nameof(configurationBuilder));
        type.VerifyNotNull(nameof(type));
        name.VerifyNotEmpty(nameof(name));

        Stream stream = Assembly.GetAssembly(type)
            ?.GetManifestResourceStream(name)
            .VerifyNotNull($"Resource {name} not found in assembly")!;

        configurationBuilder.AddJsonStream(stream);

        return configurationBuilder;
    }

    public static IConfigurationBuilder AddSource(this IConfigurationBuilder configurationBuilder, Func<IConfigurationBuilder, IConfigurationProvider> factory)
    {
        configurationBuilder.VerifyNotNull(nameof(configurationBuilder));

        configurationBuilder.Add(new ConfigurationSource(factory));

        return configurationBuilder;
    }
}
