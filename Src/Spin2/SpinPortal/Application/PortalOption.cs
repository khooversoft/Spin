using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinPortal.Application;

public record PortalOption
{
    public string SpinSiloApi { get; init; } = null!;
}

public static class PortalOptionValidator
{
    public static Validator<PortalOption> Validator { get; } = new Validator<PortalOption>()
        .RuleFor(x => x.SpinSiloApi).NotEmpty()
        .Build();

    public static ValidatorResult Validate(this PortalOption subject) => Validator.Validate(subject);

    public static PortalOption Verify(this PortalOption subject)
    {
        subject.Validate().Assert(x => x.IsValid, x => $"Option is not valid, errors={x.FormatErrors()}");
        return subject;
    }
}
