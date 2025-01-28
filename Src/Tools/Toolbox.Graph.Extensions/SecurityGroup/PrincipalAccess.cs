using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions;

public record PrincipalAccess
{
    public string PrincipalId { get; init; } = null!;
    public SecurityAccess Access { get; init; } = SecurityAccess.None;

    public static IValidator<PrincipalAccess> Validator => new Validator<PrincipalAccess>()
        .RuleFor(x => x.PrincipalId).NotEmpty()
        .RuleFor(x => x.Access).ValidEnum().Must(x => x != SecurityAccess.None, _ => "None is not allowed")
        .Build();
}

public static class PrincipalAccessTool
{
    public static Option Validate(this PrincipalAccess subject) => PrincipalAccess.Validator.Validate(subject).ToOptionStatus();

    public static Option HasAccess(this PrincipalAccess subject, SecurityAccess requireAccess) => subject.Access.HasAccess(requireAccess);
}
