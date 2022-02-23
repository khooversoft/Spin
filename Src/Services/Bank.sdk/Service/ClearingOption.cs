using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Document;
using Toolbox.Tools;

namespace Bank.sdk.Service;

public record ClearingOption
{
    public DocumentId BankDirectoryId { get; init; } = null!;

    public string BankName { get; init; } = null!;
}


public static class ClearingOptionExtensions
{
    public static ClearingOption Verify(this ClearingOption subject)
    {
        subject.VerifyNotNull(nameof(subject));

        subject.BankDirectoryId.VerifyNotNull(nameof(subject.BankDirectoryId));
        subject.BankName.VerifyNotEmpty(nameof(subject.BankName));

        return subject;
    }
}
