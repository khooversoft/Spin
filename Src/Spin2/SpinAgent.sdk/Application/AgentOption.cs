using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinAgent.sdk;

public record AgentOption
{
    public string ClusterApiUri { get; init; } = null!;
    public string AgentId { get; init; } = null!;
    public string SchedulerId { get; init; } = null!;
    public string PrincipalId { get; init; } = null!;
    public string SourceId { get; init; } = null!;

    public static IValidator<AgentOption> Validator { get; } = new Validator<AgentOption>()
        .RuleFor(x => x.ClusterApiUri).NotEmpty()
        .RuleFor(x => x.AgentId).ValidResourceId(ResourceType.System, "agent")
        .RuleFor(x => x.SchedulerId).ValidResourceId(ResourceType.System, "scheduler")
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.SourceId).ValidName()
        .Build();
}


public static class CmdOptionExtensions
{
    public static Option Validate(this AgentOption subject) => AgentOption.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this AgentOption subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static AgentOption Verify(this AgentOption subject)
    {
        subject.Validate().Assert(x => x == StatusCode.OK, x => $"Validation failed, error={x.Error}");
        return subject;
    }
}
