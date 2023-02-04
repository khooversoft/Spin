﻿using System.Collections.Generic;
using Toolbox.Abstractions.Tools;
using Toolbox.Extensions;

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
        subject.NotNull();

        subject.Banks.NotNull();
        subject.Banks.Assert(x => x?.Count > 0, "Banks list is empty");

        subject.Banks
            .ForEach(x =>
            {
                x.Value.Verify();
                x.Key.Assert(y => x.Key == x.Value.BankName, "key != BankName");
            });

        return subject;
    }

    public static BankDirectoryEntry Verify(this BankDirectoryEntry subject)
    {
        subject.NotNull();

        subject.BankName.NotEmpty(name: $"{nameof(subject.BankName)} is required");
        subject.DirectoryId.NotEmpty(name: $"{nameof(subject.DirectoryId)} is required");

        return subject;
    }
}

