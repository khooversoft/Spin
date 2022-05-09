using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Configuration
{
    public class PropertyResolverProvider : ConfigurationProvider, IPropertyResolverProvider
    {
        private readonly PropertyResolverSource _source;

        public PropertyResolverProvider(PropertyResolverSource secretConfigurationSource)
        {
            secretConfigurationSource.NotNull(nameof(secretConfigurationSource));

            _source = secretConfigurationSource;
        }


        public IConfiguration Resolve(IConfiguration configuration)
        {
            configuration.NotNull(nameof(configuration));

            IReadOnlyList<KeyValuePair<string, string>> list = configuration
                .AsEnumerable()
                .Where(x => x.Value != null)
                .ToArray();

            foreach (KeyValuePair<string, string> item in list)
            {
                if (!_source.Resolver.HasProperty(item.Value)) continue;

                Data[item.Key] = _source.Resolver.Resolve(item.Value);
            }

            return configuration;
        }
    }
}
