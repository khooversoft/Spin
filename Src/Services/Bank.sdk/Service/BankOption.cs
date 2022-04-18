using Bank.sdk.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Document;
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
        {
            bankOption.VerifyNotNull(nameof(bankOption));
            bankName.VerifyNotEmpty(nameof(bankName));

            bankName = bankName.Split('/').First();

            return bankOption.BankName.Equals(bankName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
