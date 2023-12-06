using Toolbox.Types;

namespace SpinClient.sdk;

public interface ISetupProvider
{
    Task<Option> Run(IServiceProvider serviceProvider, ScopeContext context);
}
