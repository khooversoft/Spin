using Microsoft.Extensions.Configuration;
using System.Reflection;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Property;

namespace Toolbox.Configuration;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddPropertyResolver(this IConfigurationBuilder configurationBuilder, string secretId)
    {
        configurationBuilder.NotNull();

        configurationBuilder.Add(new PropertyResolverSource(secretId));

        return configurationBuilder;
    }

    public static IConfigurationBuilder AddPropertyResolver(this IConfigurationBuilder configurationBuilder, IEnumerable<KeyValuePair<string, string>> values)
    {
        configurationBuilder.NotNull();

        configurationBuilder.Add(new PropertyResolverSource(values));

        return configurationBuilder;
    }

    public static IConfigurationBuilder AddPropertyResolver(this IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.NotNull();

        IConfiguration configuration = configurationBuilder.Build();

        // Add source
        configurationBuilder.AddSource(_ => new ResolverConfigurationProvider(configuration, configuration.BuildResolver()));

        return configurationBuilder;
    }

    public static IConfigurationBuilder AddJsonFile(this IConfigurationBuilder configurationBuilder, string file, JsonFileOption jsonFileOption, bool optional = false)
    {
        configurationBuilder.NotNull();
        file.NotEmpty();

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
        configurationBuilder.NotNull();
        type.NotNull();
        name.NotEmpty();

        Stream stream = Assembly.GetAssembly(type)
            ?.GetManifestResourceStream(name)
            .NotNull(name: $"Resource {name} not found in assembly")!;

        configurationBuilder.AddJsonStream(stream);

        return configurationBuilder;
    }

    public static IConfigurationBuilder AddSource(this IConfigurationBuilder configurationBuilder, Func<IConfigurationBuilder, IConfigurationProvider> factory)
    {
        configurationBuilder.NotNull();

        configurationBuilder.Add(new ConfigurationSource(factory));

        return configurationBuilder;
    }
}
