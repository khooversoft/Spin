using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public enum SpaceFormat
{
    None,
    Key,
    Hash,
    List
}

public class DataSpaceOption
{
    public IReadOnlyList<SpaceDefinition> Spaces { get; init; } = Array.Empty<SpaceDefinition>();
    public IReadOnlyList<IStoreProvider> Providers { get; init; } = Array.Empty<IStoreProvider>();

    public static IValidator<DataSpaceOption> Validator { get; } = new Validator<DataSpaceOption>()
        .RuleForEach(x => x.Spaces).Validate(SpaceDefinition.Validator)
        .RuleFor(x => x.Providers).Must(x => x.NotNull().Count > 0, _ => "No providers")
        .RuleForEach(x => x.Providers).Must(x => x != null, _ => "Null provider")
        .Build();
}

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

public record SpaceDefinition
{
    public string Name { get; init; } = null!;
    public string ProviderName { get; init; } = null!;
    public string BasePath { get; init; } = null!;
    public SpaceFormat SpaceFormat { get; init; }

    public static IValidator<SpaceDefinition> Validator { get; } = new Validator<SpaceDefinition>()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.ProviderName).NotEmpty()
        .RuleFor(x => x.BasePath).NotEmpty()
        .RuleFor(x => x.SpaceFormat).ValidEnum()
        .Build();
}

public static class DataSpaceOptionExtensions
{
    public static Option Validate(this DataSpaceOption option) => DataSpaceOption.Validator.Validate(option).ToOptionStatus();
}