using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShareWeb.Application;

public record AuthenticationOption
{
    public ClientSecretOption Microsoft { get; init; } = null!;
    public ClientSecretOption Google { get; init; } = null!;

    public static IValidator<AuthenticationOption> Validator { get; } = new Validator<AuthenticationOption>()
        .RuleFor(x => x.Microsoft).Validate(ClientSecretOption.Validator)
        .RuleFor(x => x.Google).Validate(ClientSecretOption.Validator)
        .Build();
}


public record ClientSecretOption
{
    public string ClientId { get; init; } = null!;
    public string ClientSecret { get; init; } = null!;

    public static IValidator<ClientSecretOption> Validator { get; } = new Validator<ClientSecretOption>()
        .RuleFor(x => x.ClientId).NotEmpty()
        .RuleFor(x => x.ClientSecret).NotEmpty()
        .Build();
}


public static class AuthenticationOptionExtensions
{
    public static Option<IValidatorResult> Validate(this AuthenticationOption option) => AuthenticationOption.Validator.Validate(option);
    public static Option<IValidatorResult> Validate(this ClientSecretOption option) => ClientSecretOption.Validator.Validate(option);
}
