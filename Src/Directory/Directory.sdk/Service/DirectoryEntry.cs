using Azure;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Toolbox.Abstractions;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Directory.sdk.Service;

public record DirectoryEntry
{
    public string DirectoryId { get; init; } = null!;

    public string ClassType { get; init; } = "default";

    public string Version { get; init; } = "v1";

    public ETag? ETag { get; init; }

    public IReadOnlyList<string> Properties { get; init; } = null!;
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

    public static string? GetEmail(this DirectoryEntry directory) => directory.Properties.GetValue(PropertyName.Email);

    public static string? GetSigningCredentials(this DirectoryEntry directory) => directory.Properties.GetValue(PropertyName.SigningCredentials);

    public static T ConvertTo<T>(this DirectoryEntry directoryEntry) where T : new() => directoryEntry.VerifyNotNull(nameof(directoryEntry))
        .Properties
        .ToConfiguration()
        .Bind<T>()
        .VerifyNotNull($"Cannot bind to {nameof(T)}");
}
