using Azure.Core;
using Azure.Storage.Files.DataLake;
using FluentValidation;
using Toolbox.Azure.Identity;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Tools.Validation.Validators;

namespace Toolbox.Azure.DataLake;

public record DatalakeOption
{
    public string AccountName { get; init; } = null!;
    public string ContainerName { get; init; } = null!;
    public string? BasePath { get; init; }
    public ClientSecretOption Credentials { get; init; } = null!;
}


public static class DatalakeOptionExtensions
{
    public static Validator<DatalakeOption> Validator = new Validator<DatalakeOption>()
        .RuleFor(x => x.AccountName).NotEmpty()
        .RuleFor(x => x.ContainerName).NotEmpty()
        .RuleFor(x => x.Credentials).Validate(ClientSecretOptionValidator.Validator)
        .Build();

    public static ValidatorResult<DatalakeOption> Validate(this DatalakeOption subject) => Validator.Validate(subject);

    public static DataLakeServiceClient CreateDataLakeServiceClient(this DatalakeOption subject)
    {
        subject.NotNull();

        var serviceUri = new Uri($"https://{subject.AccountName}.blob.core.windows.net");
        TokenCredential credential = subject.Credentials.ToTokenCredential();

        return new DataLakeServiceClient(serviceUri, credential);
    }
}