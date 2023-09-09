using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Models;

[GenerateSerializer, Immutable]
public class AgentAssignmentModel
{
    [Id(0)] public string AgentId { get; init; } = null!;
    [Id(1)] public string WorkId { get; init; } = null!;
    [Id(2)] public string SmartcId { get; init; } = null!;
    [Id(3)] public string CommandType { get; init; } = null!;
    [Id(4)] public string Command { get; init; } = null!;

    public static IValidator<AgentAssignmentModel> Validator { get; } = new Validator<AgentAssignmentModel>()
        //.RuleFor(x => x.AgentId).ValidResourceId(ResourceType.System, "agent")
        //.RuleFor(x => x.WorkId).NotEmpty()
        //.RuleFor(x => x.AgentId).ValidResourceId(ResourceType.DomainOwned, "smartc")
        //.RuleFor(x => x.CommandType).NotEmpty()
        .RuleFor(x => x.Command).NotEmpty()
        .Build();
}


public static class AgentAssignmentModelExtensions
{
    public static Option Validate(this AgentAssignmentModel subject) => AgentAssignmentModel.Validator.Validate(subject).ToOptionStatus();
}