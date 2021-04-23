using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Tools;
using Toolbox.Tools.PropertyResolver;

namespace Toolbox.Configuration
{
    public static class Extensions
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
            values.VerifyNotNull(nameof(values));

            configurationBuilder.Add(new PropertyResolverSource(values));

            return configurationBuilder;
        }

        public static IConfiguration ResolveProperties(this IConfigurationRoot configuration)
        {
            configuration.Providers
                .OfType<IPropertyResolverProvider>()
                .FirstOrDefault()
                ?.Resolve(configuration);

            return configuration;
        }
    }
}