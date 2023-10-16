using Microsoft.Extensions.Configuration;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Property;

namespace Toolbox.Configuration
{
    public class PropertyResolverSource : IConfigurationSource
    {
        private readonly string? _secretId;
        private readonly IEnumerable<KeyValuePair<string, string>>? _values;

        public PropertyResolverSource(string secretId)
        {
            secretId.NotEmpty();

            _secretId = secretId;
        }

        public PropertyResolverSource(IEnumerable<KeyValuePair<string, string>> values)
        {
            values.NotNull();

            _values = values;
        }

        public Dictionary<string, string> InitialData { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IPropertyResolver Resolver { get; private set; } = null!;

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            Resolver = _secretId.IsNotEmpty() switch
            {
                true => new PropertyResolver(PropertySecret.ReadFromSecret(_secretId).Properties),
                false => new PropertyResolver(_values!),
            };

            return new PropertyResolverProvider(this);
        }
    }
}