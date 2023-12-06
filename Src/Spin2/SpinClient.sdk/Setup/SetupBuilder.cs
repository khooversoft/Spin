using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClient.sdk;

public class SetupBuilder
{
    private Sequence<ISetupProvider> _setupProviders = new();

    public SetupBuilder AddProvider(ISetupProvider provider) => this.Action(_ => _setupProviders.Add(provider.NotNull()));
    public SetupBuilder AddProvider(Func<IServiceProvider, ScopeContext, Task<Option>> func)
    {
        _setupProviders.Add(new SetupProxy(func));
        return this;
    }


    private readonly struct SetupProxy : ISetupProvider
    {
        private readonly Func<IServiceProvider, ScopeContext, Task<Option>> _func;
        public SetupProxy(Func<IServiceProvider, ScopeContext, Task<Option>> func) => _func = func;
        public Task<Option> Run(IServiceProvider serviceProvider, ScopeContext context) => _func(serviceProvider, context);
    }
}

