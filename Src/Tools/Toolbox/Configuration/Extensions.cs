using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Toolbox.Extensions;
using Toolbox.Model;
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
        configuration.VerifyNotNull(nameof(configuration));

        IReadOnlyList<KeyValuePair<string, string>> list = configuration
            .AsEnumerable()
            .Where(x => x.Value != null)
            .ToArray();

        return new PropertyResolver(list);
    }
}
