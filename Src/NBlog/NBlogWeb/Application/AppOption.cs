using Toolbox.Azure.Identity;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlogWeb.Application;

public class UserSecretName
{
    public string AppInsightsConnectionString { get; init; } = null!;
    public string? UserSecrets { get; init; }
}


public record AppOption
{
    public ClientSecretOption Credentials { get; init; } = null!;

    public static Validator<AppOption> Validator { get; } = new Validator<AppOption>()
        .RuleFor(x => x.Credentials).Validate(ClientSecretOption.Validator)
        .Build();
}

public static class AppOptionValidator
{
    public static Option Validate(this AppOption subject) => AppOption.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this AppOption subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}