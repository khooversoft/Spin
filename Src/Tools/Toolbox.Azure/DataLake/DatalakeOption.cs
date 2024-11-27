using Azure.Core;
using Azure.Storage.Files.DataLake;
using Microsoft.Extensions.Configuration;
using Toolbox.Azure.Identity;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

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


public static class DatalakeOptionTool
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

    public static DatalakeOption Get(IConfiguration configuration)
    {
        configuration.NotNull();

        ClientSecretOption secretOption = configuration.GetConnectionString("AppConfig").NotNull()
            .ToDictionaryFromString()
            .ToObject<ClientSecretOption>();

        DatalakeOption datalakeOption = configuration.GetSection("Storage").Get<DatalakeOption>().NotNull();

        datalakeOption = datalakeOption with
        {
            Credentials = secretOption,
        };

        datalakeOption.Validate().ThrowOnError("Invalid DatalakeOption");
        return datalakeOption;
    }

    public static DatalakeOption Create(string datalakeConnectionString, string clientSecretConnectionString)
    {
        var datalakeOptionDict = datalakeConnectionString.ToDictionaryFromString();
        var datalakeOption = datalakeOptionDict.ToObject<DatalakeOption>();

        var dictClientSecretDict = clientSecretConnectionString.ToDictionaryFromString();
        var clientSecretOption = dictClientSecretDict.ToObject<ClientSecretOption>();

        datalakeOption = datalakeOption with
        {
            Credentials = clientSecretOption,
        };

        datalakeOption.Validate().ThrowOnError("Invalid DatalakeOption, correct connection strings");
        return datalakeOption;
    }
}
