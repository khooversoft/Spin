using Azure;
using Directory.sdk.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Document;
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
        subject.DirectoryId.VerifyDocumentId();
        subject.ClassType.VerifyNotEmpty(nameof(subject.ClassType));
        subject.Properties.VerifyNotNull(nameof(subject.Properties));
    }

    public static string? GetPropertyValue(this DirectoryEntry directoryEntry, string name) => directoryEntry
            .Properties.Values
            .FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            ?.Value;

    public static string? GetEmail(this DirectoryEntry directory) => GetPropertyValue(directory, PropertyName.Email);

    public static string? GetSigningCredentials(this DirectoryEntry directory) => GetPropertyValue(directory, PropertyName.SigningCredentials);
}
