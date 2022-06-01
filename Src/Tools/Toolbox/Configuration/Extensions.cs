using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Tools;
using Toolbox.Tools.Property;

namespace Toolbox.Configuration;

public enum JsonFileOption
{
    None = 0,
    Enhance = 1
}

public static class Extensions
{
    public static IConfiguration ResolveProperties(this IConfigurationRoot configuration)
    {
        configuration.Providers
            .OfType<IPropertyResolverProvider>()
            .FirstOrDefault()
            ?.Resolve(configuration);

        return configuration;
    }

    public static IPropertyResolver BuildResolver(this IConfiguration configuration)
    {
        configuration.NotNull();

        IReadOnlyList<KeyValuePair<string, string>> list = configuration
            .AsEnumerable()
            .Where(x => x.Value != null)
            .ToArray();

        return new PropertyResolver(list);
    }
}
