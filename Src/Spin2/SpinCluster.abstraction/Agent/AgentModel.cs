using Orleans;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.abstraction;

// agent:{name}
[GenerateSerializer, Immutable]
public record AgentModel
{
    [Id(0)] public string AgentId { get; init; } = null!;
    [Id(1)] public DateTime Registered { get; init; } = DateTime.UtcNow;
    [Id(2)] public bool Enabled { get; init; }
    [Id(3)] public string WorkingFolder { get; init; } = @"c:\spinAgent";

    public bool IsActive => Enabled;

    public static IValidator<AgentModel> Validator { get; } = new Validator<AgentModel>()
        .RuleFor(x => x.AgentId).ValidResourceId(ResourceType.System, "agent")
        .RuleFor(x => x.WorkingFolder).NotEmpty()
        .Build();
}


public static class AgentModelExtensions
{
    public static Option Validate(this AgentModel model) => AgentModel.Validator.Validate(model).ToOptionStatus();

    public static bool Validate(this AgentModel model, out Option result)
    {
        result = model.Validate();
        return result.IsOk();
    }
}