using Bank.Abstractions.Model;
using Toolbox.Application;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Bank.sdk.Service;

public record BankOption
{
    public RunEnvironment RunEnvironment { get; init; }

    public string BankName { get; init; } = null!;

    public string ArtifactContainerName { get; init; } = null!;
}


public static class ClearingOptionExtensions
{
    public static BankOption Verify(this BankOption subject)
    {
        subject.VerifyNotNull(nameof(subject));

        subject.RunEnvironment.VerifyAssert(x => x != RunEnvironment.Unknown, "Environment is required");
        subject.BankName.VerifyNotEmpty(nameof(subject.BankName));
        subject.ArtifactContainerName.VerifyNotEmpty(nameof(subject.ArtifactContainerName));

        return subject;
    }

    public static bool IsBankName(this BankOption bankOption, string bankName)
    {
        bankOption.VerifyNotNull(nameof(bankOption));
        bankName.VerifyNotEmpty(nameof(bankName));

        bankName = bankName.Split('/').First();

        return bankOption.BankName.EqualsIgnoreCase(bankName);
    }

    public static bool IsForLocalHost(this BankOption bankOption, TrxRequest trxRequest) =>
        bankOption.IsBankName(trxRequest.ToId) || bankOption.IsBankName(trxRequest.FromId);

    public static string GetLocalId(this BankOption bankOption, TrxRequest trxRequest) =>
        bankOption.IsBankName(trxRequest.ToId) ? trxRequest.ToId : trxRequest.FromId;
}
