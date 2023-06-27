using Toolbox.Extensions;
using Toolbox.Tools.Validation;

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

    public static ValidatorResult Validate(this PortalOption option) => Validator.Validate(option);

    public static PortalOption Verify(this PortalOption option) => option.Action(x => x.Validate().ThrowOnError());
}
