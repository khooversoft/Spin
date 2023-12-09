using Microsoft.Extensions.Configuration;
using Toolbox.Tools;

namespace Toolbox.Configuration
{
    public class ConfigurationSource : IConfigurationSource
    {
        private readonly Func<IConfigurationBuilder, IConfigurationProvider> _factory;

        public ConfigurationSource(Func<IConfigurationBuilder, IConfigurationProvider> factory)
        {
            factory.NotNull();

            _factory = factory;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder) => _factory(builder);
    }
}
