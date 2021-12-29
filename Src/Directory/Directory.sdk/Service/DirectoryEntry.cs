﻿using Azure;
using Directory.sdk.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Directory.sdk.Service;

public record DirectoryEntry
{
    public string DirectoryId { get; init; } = null!;

    public string ClassType { get; init; } = "default";

    public string Version { get; init; } = "v1";

    public ETag? ETag { get; init; }

    public IDictionary<string, EntryProperty> Properties { get; init; } = new Dictionary<string, EntryProperty>(StringComparer.OrdinalIgnoreCase);
}


public static class DirectoryEntryExtensions
{
    public static void Verify(this DirectoryEntry subject)
    {
        subject.VerifyNotNull(nameof(subject));
        subject.DirectoryId.VerifyDirectoryId();
        subject.ClassType.VerifyNotEmpty(nameof(subject.ClassType));
    }
}
