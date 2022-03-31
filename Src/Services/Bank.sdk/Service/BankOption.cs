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
}


public static class ClearingOptionExtensions
{
    public static BankOption Verify(this BankOption subject)
    {
        subject.VerifyNotNull(nameof(subject));

        subject.RunEnvironment.VerifyAssert(x => x != RunEnvironment.Unknown, "Environment is required");
        subject.BankName.VerifyNotEmpty(nameof(subject.BankName));

        return subject;
    }
}
