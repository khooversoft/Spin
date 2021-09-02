using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Toolbox.Tools;
using Toolbox.Tools.Property;

namespace Toolbox.Configuration
{
    public class ResolverConfigurationProvider : ConfigurationProvider
    {
        private readonly IConfiguration _configuration;
        private readonly IPropertyResolver _propertyResolver;

        public ResolverConfigurationProvider(IConfiguration configuration, IPropertyResolver propertyResolver)
        {
            configuration.VerifyNotNull(nameof(configuration));
            propertyResolver.VerifyNotNull(nameof(propertyResolver));

            _configuration = configuration;
            _propertyResolver = propertyResolver;
        }

        public override void Load()
        {
            IReadOnlyList<KeyValuePair<string, string>> list = _configuration
                .AsEnumerable()
                .Where(x => x.Value != null)
                .ToArray();

            foreach (KeyValuePair<string, string> item in list)
            {
                if (!_propertyResolver.HasProperty(item.Value)) continue;

                Data[item.Key] = _propertyResolver.Resolve(item.Value);
            }
        }
    }
}
