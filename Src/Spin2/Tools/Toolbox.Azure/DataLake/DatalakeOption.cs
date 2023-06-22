using Azure.Core;
using Azure.Storage.Files.DataLake;
using Toolbox.Azure.Identity;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;

namespace Toolbox.Azure.DataLake;

public record DatalakeOption
{
    public string AccountName { get; init; } = null!;
    public string ContainerName { get; init; } = null!;
    public string? BasePath { get; init; }
    public ClientSecretOption Credentials { get; init; } = null!;
}


public static class DatalakeOptionValidator
{
    public static Validator<DatalakeOption> Validator { get; } = new Validator<DatalakeOption>()
        .RuleFor(x => x.AccountName).NotEmpty()
        .RuleFor(x => x.ContainerName).NotEmpty()
        .RuleFor(x => x.Credentials).Validate(ClientSecretOptionValidator.Validator)
        .Build();

    public static ValidatorResult Validate(this DatalakeOption subject) => Validator.Validate(subject);

    public static DatalakeOption Verify(this DatalakeOption subject) => subject.Action(x => x.Validate().ThrowOnError());

    public static DataLakeServiceClient CreateDataLakeServiceClient(this DatalakeOption subject)
    {
        subject.NotNull();

        var serviceUri = new Uri($"https://{subject.AccountName}.blob.core.windows.net");
        TokenCredential credential = subject.Credentials.ToTokenCredential();

        return new DataLakeServiceClient(serviceUri, credential);
    }
}