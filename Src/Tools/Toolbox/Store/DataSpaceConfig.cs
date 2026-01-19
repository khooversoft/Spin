using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Store;

public class DataSpaceConfig
{
    public List<SpaceDefinition> Spaces { get; init; } = new();
    public List<IStoreProvider> Providers { get; init; } = new();
    public List<Func<IServiceProvider, IStoreProvider>> ProviderFactories { get; init; } = new();

    public DataSpaceConfig Add(SpaceDefinition subject) => this.Action(_ => Spaces.Add(subject.NotNull()));

    public DataSpaceConfig Add(IStoreProvider subject) => this.Action(_ => Providers.Add(subject.NotNull()));

    public DataSpaceConfig Add<T>(string name) where T : IStoreProvider
    {
        name.NotEmpty();
        ProviderFactories.Add(x => ActivatorUtilities.CreateInstance<T>(x, name));
        return this;
    }

    public DataSpaceConfig Add<T>(Func<IServiceProvider, T> factory) where T : IStoreProvider
    {
        ProviderFactories.Add(x => factory(x));
        return this;
    }

    public DataSpaceOption Build(IServiceProvider serviceProvider) => new()
    {
        Spaces = Spaces.ToImmutableArray(),
        Providers = Providers.Concat(ProviderFactories.Select(x => x(serviceProvider))).ToImmutableArray(),
    };
}
