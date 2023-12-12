using Azure.Core;
using Azure.Storage.Files.DataLake;
using Toolbox.Azure.Identity;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure.DataLake;

public record DatalakeOption
{
    public string Account { get; init; } = null!;
    public string Container { get; init; } = null!;
    public string? BasePath { get; init; }
    public ClientSecretOption Credentials { get; init; } = null!;

    public override string ToString() => $"Account={Account}, Container={Container}, BasePath={BasePath}, Credentials={Credentials}";

    public static IValidator<DatalakeOption> Validator { get; } = new Validator<DatalakeOption>()
        .RuleFor(x => x.Account).NotEmpty()
        .RuleFor(x => x.Container).NotEmpty()
        .RuleFor(x => x.Credentials).Validate(ClientSecretOption.Validator)
        .Build();
}


public static class DatalakeOptionValidator
{
    public static Option Validate(this DatalakeOption subject) => DatalakeOption.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this DatalakeOption subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static DataLakeServiceClient CreateDataLakeServiceClient(this DatalakeOption subject)
    {
        subject.NotNull();

        var serviceUri = new Uri($"https://{subject.Account}.blob.core.windows.net");
        TokenCredential credential = subject.Credentials.ToTokenCredential();

        return new DataLakeServiceClient(serviceUri, credential);
    }
}