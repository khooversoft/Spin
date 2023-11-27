using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinAgent.Application;

public record AgentOption
{
    public string ClusterApiUri { get; init; } = null!;
    public string AgentId { get; init; } = null!;
    public string SchedulerId { get; init; } = null!;
}


public static class CmdOptionExtensions
{
    public static Validator<AgentOption> Validator { get; } = new Validator<AgentOption>()
        .RuleFor(x => x.ClusterApiUri).NotEmpty()
        .RuleFor(x => x.AgentId).ValidResourceId(ResourceType.System, "agent")
        .RuleFor(x => x.SchedulerId).ValidResourceId(ResourceType.System, "scheduler")
        .Build();

    public static AgentOption Verify(this AgentOption option) => option.Action(x => Validator.Validate(x).ThrowOnError());
}
