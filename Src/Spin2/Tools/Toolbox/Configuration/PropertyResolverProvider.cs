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
            secretConfigurationSource.NotNull();

            _source = secretConfigurationSource;
        }


        public IConfiguration Resolve(IConfiguration configuration)
        {
            configuration.NotNull();

            IReadOnlyList<KeyValuePair<string, string>> list = configuration
                .AsEnumerable()
                .Where(x => x.Value != null)
                .OfType<KeyValuePair<string, string>>()
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
