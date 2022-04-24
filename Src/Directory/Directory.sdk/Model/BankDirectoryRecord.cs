using System.Collections.Generic;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Directory.sdk.Model;

public record BankDirectoryRecord
{
    public IReadOnlyDictionary<string, BankDirectoryEntry> Banks { get; set; } = null!;
}

public record BankDirectoryEntry
{
    public string BankName { get; set; } = null!;

    public string DirectoryId { get; set; } = null!;
}



public static class BankDirectoryRecordExtensions
{
    public static BankDirectoryRecord Verify(this BankDirectoryRecord subject)
    {
        subject.VerifyNotNull(nameof(subject));

        subject.Banks.VerifyNotNull(nameof(subject.Banks));
        subject.Banks.VerifyAssert(x => x?.Count > 0, "Banks list is empty");

        subject.Banks
            .ForEach(x =>
            {
                x.Value.Verify();
                x.Key.VerifyAssert(y => x.Key == x.Value.BankName, "key != BankName");
            });

        return subject;
    }

    public static BankDirectoryEntry Verify(this BankDirectoryEntry subject)
    {
        subject.VerifyNotNull(nameof(subject));

        subject.BankName.VerifyNotEmpty($"{nameof(subject.BankName)} is required");
        subject.DirectoryId.VerifyNotEmpty($"{nameof(subject.DirectoryId)} is required");

        return subject;
    }
}

