using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.abstraction;

[GenerateSerializer, Immutable]
public sealed record RemovePropertyModel
{
    [Id(0)] public string ConfigId { get; init; } = null!;
    [Id(1)] public string Key { get; init; } = null!;

    public static IValidator<RemovePropertyModel> Validator { get; } = new Validator<RemovePropertyModel>()
        .RuleFor(x => x.ConfigId).ValidResourceId(ResourceType.System, SpinConstants.Schema.Config)
        .RuleFor(x => x.Key).NotEmpty()
        .Build();
}

public static class DeletePropertyModelExtensions
{
    public static Option Validate(this RemovePropertyModel subject) => RemovePropertyModel.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this RemovePropertyModel subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}