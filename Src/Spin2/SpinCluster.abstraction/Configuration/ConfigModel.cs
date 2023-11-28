using Orleans;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.abstraction;


[GenerateSerializer, Immutable]
public sealed record ConfigModel
{
    // spinconfig:{configId}
    [Id(0)] public string ConfigId { get; init; } = null!;
    [Id(1)] public IReadOnlyDictionary<string, string> Properties { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public bool Equals(ConfigModel? obj) => obj is ConfigModel document &&
        ConfigId == document.ConfigId &&
        Enumerable.SequenceEqual(Properties.OrderBy(x => x.Key), document.Properties.OrderBy(x => x.Key));

    public override int GetHashCode() => HashCode.Combine(ConfigId, Properties);

    public static IValidator<ConfigModel> Validator { get; } = new Validator<ConfigModel>()
        .RuleFor(x => x.ConfigId).ValidResourceId(ResourceType.System, SpinConstants.Schema.Config)
        .RuleFor(x => x.Properties).NotNull()
        .Build();
}

public static class ConfigModelExtensions
{
    public static Option Validate(this ConfigModel subject) => ConfigModel.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ConfigModel subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}

