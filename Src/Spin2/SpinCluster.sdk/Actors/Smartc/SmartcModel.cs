using SpinCluster.sdk.Models;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Smartc;

// smartc:{name}
[GenerateSerializer, Immutable]
public record SmartcModel
{
    [Id(0)] public string SmartcId { get; init; } = null!;
    [Id(1)] public DateTime Registered { get; init; } = DateTime.UtcNow;
    [Id(2)] public string SmartcExeId { get; init; } = null!;
    [Id(3)] public string ContractId { get; init; } = null!;
    [Id(4)] public bool Enabled { get; init; }
    [Id(5)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(6)] public AgentAssignmentModel? Assignment { get; init; }

    public bool IsActive => Enabled;

    public static IValidator<SmartcModel> Validator { get; } = new Validator<SmartcModel>()
        .RuleFor(x => x.SmartcId).ValidResourceId(ResourceType.DomainOwned, "smartc")
        .RuleFor(x => x.Assignment).ValidateOption(AgentAssignmentModel.Validator)
        .RuleFor(x => x.SmartcExeId).ValidResourceId(ResourceType.DomainOwned, "smartc-exe")
        .RuleFor(x => x.ContractId).ValidResourceId(ResourceType.DomainOwned, "contract")
        .Build();
}


public static class SmartcModelExtensions
{
    public static Option Validate(this SmartcModel model) => SmartcModel.Validator.Validate(model).ToOptionStatus();
}