using Microsoft.Extensions.Configuration;

namespace Toolbox.Configuration
{
    public interface IPropertyResolverProvider
    {
        IConfiguration Resolve(IConfiguration configuration);
    }
}