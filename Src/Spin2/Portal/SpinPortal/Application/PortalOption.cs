using Toolbox.Extensions;
using Toolbox.Tools.Validation;

namespace SpinPortal.Application;

public record PortalOption
{
    public string DirectoryUri { get; init; } = null!;
    public IReadOnlyList<string> Domains { get; init; } = Array.Empty<string>();
}

public static class PortalOptionValidator
{
    public static Validator<PortalOption> Validator { get; } = new Validator<PortalOption>()
        .RuleFor(x => x.DirectoryUri).NotEmpty()
        .RuleForEach(x => x.Domains).NotEmpty()
        .RuleFor(x => x.Domains).Must(x => x.Count > 0, _ => $"Domain list is empty")
        .Build();

    public static ValidatorResult Validate(this PortalOption option) => Validator.Validate(option);

    public static PortalOption Verify(this PortalOption option) => option.Action(x => x.Validate().ThrowOnError());
}
