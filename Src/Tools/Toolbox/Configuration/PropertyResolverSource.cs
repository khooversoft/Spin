using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.PropertyResolver;

namespace Toolbox.Configuration
{
    public class PropertyResolverSource : IConfigurationSource
    {
        private readonly string? _secretId;
        private readonly IEnumerable<KeyValuePair<string, string>>? _values;

        public PropertyResolverSource(string secretId)
        {
            secretId.VerifyNotEmpty(nameof(secretId));

            _secretId = secretId;
        }

        public PropertyResolverSource(IEnumerable<KeyValuePair<string, string>> values)
        {
            values.VerifyNotNull(nameof(values));

            _values = values;
        }

        public Dictionary<string, string> InitialData { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IPropertyResolver Resolver { get; private set; } = null!;

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            Resolver = !_secretId.IsEmpty()
                ? new PropertyResolver(new PropertyResolverBuilder().LoadFromSecretId(_secretId).List())
                : new PropertyResolver(_values!);

            return new PropertyResolverProvider(this);
        }
    }
}
