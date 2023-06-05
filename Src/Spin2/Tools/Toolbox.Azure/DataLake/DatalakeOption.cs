using Azure.Core;
using Azure.Storage.Files.DataLake;
using FluentValidation;
using Toolbox.Azure.Identity;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Azure.DataLake;

public record DatalakeOption
{
    public string AccountName { get; init; } = null!;
    public string ContainerName { get; init; } = null!;
    public string? BasePath { get; init; }
    public ClientSecretOption Credentials { get; init; } = null!;
}


public class DatalakeOptionValidator : AbstractValidator<DatalakeOption>
{
    public DatalakeOptionValidator()
    {
        RuleFor(x => x.AccountName).NotEmpty();
        RuleFor(x => x.Credentials).SetValidator(ClientSecretOptionValidator.Default);
    }

    public static DatalakeOptionValidator Default { get; } = new DatalakeOptionValidator();
}


public static class DatalakeOptionExtensions
{
    public static DatalakeOption Verify(this DatalakeOption subject) => DatalakeOptionValidator.Default.Verify(subject);
    public static bool IsVerify(this DatalakeOption subject) => DatalakeOptionValidator.Default.IsValid(subject);
    public static IReadOnlyList<string> GetVerifyErrors(this DatalakeOption subject) => DatalakeOptionValidator.Default.GetErrors(subject);

    public static DataLakeServiceClient CreateDataLakeServiceClient(this DatalakeOption subject)
    {
        subject.NotNull();

        var serviceUri = new Uri($"https://{subject.AccountName}.blob.core.windows.net");
        TokenCredential credential = subject.Credentials.ToTokenCredential();

        return new DataLakeServiceClient(serviceUri, credential);
    }
}