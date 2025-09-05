using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShareWeb.Application;

public record AppRegistrationOption
{
    public string VaultUri { get; init; } = null!;
    public string TenantId { get; init; } = null!;
    public string ClientId { get; init; } = null!;
    public string? ClientSecret { get; init; }

    public static IValidator<AppRegistrationOption> Validator { get; } = new Validator<AppRegistrationOption>()
        .RuleFor(x => x.VaultUri).NotEmpty()
        .RuleFor(x => x.TenantId).NotEmpty()
        .RuleFor(x => x.ClientId).NotEmpty()
        .Build();
}


public static class AppRegistrationOptionExtensions
{
    public static Option<IValidatorResult> Validate(this AppRegistrationOption option) => AppRegistrationOption.Validator.Validate(option);
}
