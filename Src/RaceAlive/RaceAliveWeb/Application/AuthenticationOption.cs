using Toolbox.Tools;
using Toolbox.Types;

namespace RaceAliveWeb.Application;

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

public record AppleSecretOption
{
    public string ClientId { get; init; } = null!;
    public string TeamId { get; init; } = null!;
    public string KeyId { get; init; } = null!;
    public string PrivateKeyPath { get; init; } = null!;

    public static IValidator<AppleSecretOption> Validator { get; } = new Validator<AppleSecretOption>()
        .RuleFor(x => x.ClientId).NotEmpty()
        .RuleFor(x => x.TeamId).NotEmpty()
        .RuleFor(x => x.KeyId).NotEmpty()
        .RuleFor(x => x.PrivateKeyPath).NotEmpty()
        .Build();
}


public static class AuthenticationOptionExtensions
{
    public static Option<IValidatorResult> Validate(this AuthenticationOption option) => AuthenticationOption.Validator.Validate(option);
    public static Option<IValidatorResult> Validate(this ClientSecretOption option) => ClientSecretOption.Validator.Validate(option);
}
