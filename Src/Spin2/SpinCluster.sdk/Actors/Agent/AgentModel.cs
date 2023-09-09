using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Agent;

// agent:{name}
[GenerateSerializer, Immutable]
public record AgentModel
{
    [Id(0)] public string AgentId { get; init; } = null!;
    [Id(1)] public DateTime Registered { get; init; } = DateTime.UtcNow;
    [Id(2)] public bool Enabled { get; init; }

    public static IValidator<AgentModel> Validator { get; } = new Validator<AgentModel>()
        .RuleFor(x => x.AgentId).ValidResourceId(ResourceType.System, "agent")
        .Build();
}


public static class AgentModelExtensions
{
    public static Option Validate(this AgentModel model) => AgentModel.Validator.Validate(model).ToOptionStatus();
}