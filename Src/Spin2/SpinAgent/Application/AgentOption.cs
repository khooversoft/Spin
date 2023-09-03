using SpinAgent.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinAgent.Application;

internal record AgentOption
{
    public string ClusterApi { get; init; } = null!;
    public string AgentId { get; init; } = null!;
    public string CommandLine { get; init; } = null!;
}


internal static class CmdOptionExtensions
{
    public static Validator<AgentOption> Validator { get; } = new Validator<AgentOption>()
        .RuleFor(x => x.ClusterApi).NotEmpty()
        .RuleFor(x => x.AgentId).NotEmpty()
        .RuleFor(x => x.CommandLine).NotEmpty()
        .Build();

    public static AgentOption Verify(this AgentOption option) => option.Action(x => Validator.Validate(x).ThrowOnError());
}
