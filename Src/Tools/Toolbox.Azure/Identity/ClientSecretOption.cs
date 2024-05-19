using Azure.Core;
using Azure.Identity;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure.Identity;

public record ClientSecretOption
{
    public string TenantId { get; init; } = null!;
    public string ClientId { get; init; } = null!;
    public string ClientSecret { get; init; } = null!;

    public override string ToString() => $"TenantId={TenantId}, ClientId={ClientId}, ClientSecret={ClientSecret.GetSecretThumbprint()}";

    public static IValidator<ClientSecretOption> Validator { get; } = new Validator<ClientSecretOption>()
        .RuleFor(x => x.TenantId).NotEmpty()
        .RuleFor(x => x.ClientId).NotEmpty()
        .RuleFor(x => x.ClientSecret).NotEmpty()
        .Build();
}


public static class ClientSecretOptionTool
{
    public static Option Validate(this ClientSecretOption subject) => ClientSecretOption.Validator.Validate(subject).ToOptionStatus();

    public static TokenCredential ToTokenCredential(this ClientSecretOption subject) =>
        new ClientSecretCredential(subject.TenantId, subject.ClientId, subject.ClientSecret);

    public static ClientSecretOption Create(string connectionString)
    {
        var dict = connectionString.ToDictionaryFromString();
        var option = dict.ToObject<ClientSecretOption>();
        return option;
    }
}