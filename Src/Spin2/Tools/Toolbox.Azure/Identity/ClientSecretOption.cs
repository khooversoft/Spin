using Azure.Core;
using Azure.Identity;
using FluentValidation;

namespace Toolbox.Azure.Identity;

public record ClientSecretOption
{
    public string TenantId { get; init; } = null!;
    public string ClientId { get; init; } = null!;
    public string ClientSecret { get; init; } = null!;
}

public class ClientSecretOptionValidator : AbstractValidator<ClientSecretOption>
{
    public static ClientSecretOptionValidator Default { get; } = new ClientSecretOptionValidator();

    public ClientSecretOptionValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ClientSecret).NotEmpty();
    }
}


public static class ClientSecretOptionExtensions
{
    public static void Verify(this ClientSecretOption subject) =>
        ClientSecretOptionValidator.Default.ValidateAndThrow(subject);

    public static bool IsVerify(this ClientSecretOption subject) =>
        ClientSecretOptionValidator.Default.Validate(subject).IsValid;

    public static IReadOnlyList<string> GetVerifyErrors(this ClientSecretOption subject) => ClientSecretOptionValidator.Default
        .Validate(subject)
        .Errors
        .Select(x => x.ErrorMessage)
        .ToArray();

    public static TokenCredential ToTokenCredential(this ClientSecretOption subject) =>
        new ClientSecretCredential(subject.TenantId, subject.ClientId, subject.ClientSecret);
}