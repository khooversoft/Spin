using Azure.Core;
using Azure.Identity;
using FluentValidation;
using Toolbox.Tools.Validation;
using Toolbox.Tools.Validation.Validators;

namespace Toolbox.Azure.Identity;

public record ClientSecretOption
{
    public string TenantId { get; init; } = null!;
    public string ClientId { get; init; } = null!;
    public string ClientSecret { get; init; } = null!;
}


public static class ClientSecretOptionValidator
{
    public static Validator<ClientSecretOption> Validator = new Validator<ClientSecretOption>()
        .RuleFor(x => x.TenantId).NotEmpty()
        .RuleFor(x => x.ClientId).NotEmpty()
        .RuleFor(x => x.ClientSecret).NotEmpty()
        .Build();

    public static ValidatorResult Validate(this ClientSecretOption subject) => Validator.Validate(subject);

    public static TokenCredential ToTokenCredential(this ClientSecretOption subject) =>
        new ClientSecretCredential(subject.TenantId, subject.ClientId, subject.ClientSecret);
}