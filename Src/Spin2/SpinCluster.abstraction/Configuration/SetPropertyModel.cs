using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.abstraction;

[GenerateSerializer, Immutable]
public sealed record SetPropertyModel
{
    [Id(0)] public string ConfigId { get; init; } = null!;
    [Id(1)] public string Key { get; init; } = null!;
    [Id(2)] public string Value { get; init; } = null!;

    public static IValidator<SetPropertyModel> Validator { get; } = new Validator<SetPropertyModel>()
        .RuleFor(x => x.ConfigId).ValidResourceId(ResourceType.System, SpinConstants.Schema.Config)
        .RuleFor(x => x.Key).NotEmpty()
        .RuleFor(x => x.Value).NotEmpty()
        .Build();
}

public static class AddPropertyModelExtensions
{
    public static Option Validate(this SetPropertyModel subject) => SetPropertyModel.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this SetPropertyModel subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}