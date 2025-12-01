using Toolbox.Data;
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
    public Sequence<SpaceDefinition> Spaces { get; init; } = new();
    public Sequence<IStoreProvider> Providers { get; init; } = new();

    public static IValidator<DataSpaceOption> Validator { get; } = new Validator<DataSpaceOption>()
        .RuleForEach(x => x.Spaces).Validate(SpaceDefinition.Validator)
        .RuleFor(x => x.Providers).Must(x => x.NotNull().Count > 0, _ => "No providers")
        .RuleForEach(x => x.Providers).Must(x => x != null, _ => "Null provider")
        .Build();
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