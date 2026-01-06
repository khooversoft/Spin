using Azure.Core;
using Azure.Storage.Files.DataLake;
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

    public static DataLakeServiceClient CreateDataLakeServiceClient(this DatalakeOption subject)
    {
        subject.NotNull();

        var serviceUri = new Uri($"https://{subject.Account}.blob.core.windows.net");
        TokenCredential credential = subject.Credentials.ToTokenCredential();

        return new DataLakeServiceClient(serviceUri, credential);
    }

    public static string WithBasePath(this DatalakeOption subject, string? path)
    {
        var result = (subject.BasePath, path) switch
        {
            (string v, null) => v,
            (string v1, string v2) when v2.StartsWith(v1, StringComparison.OrdinalIgnoreCase) => v2,
            (string v1, string v2) => (v1 + "/" + v2)
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Join('/'),

            _ => throw new ArgumentException("BasePath and Path is null"),
        };

        return result.ToLowerInvariant();
    }

    public static string RemoveBaseRoot(this DatalakeOption subject, string path)
    {
        string newPath = path[(subject.BasePath?.Length ?? 0)..];
        if (newPath.StartsWith("/")) newPath = newPath[1..];

        return newPath;
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
