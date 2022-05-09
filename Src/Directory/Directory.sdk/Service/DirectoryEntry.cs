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
        subject.NotNull(nameof(subject));
        subject.DirectoryId.VerifyDocumentId();
        subject.ClassType.NotEmpty(nameof(subject.ClassType));
        subject.Properties.NotNull(nameof(subject.Properties));
    }

    public static string? GetEmail(this DirectoryEntry directory) => directory.Properties.GetValue(PropertyName.Email);

    public static string? GetSigningCredentials(this DirectoryEntry directory) => directory.Properties.GetValue(PropertyName.SigningCredentials);

    public static T ConvertTo<T>(this DirectoryEntry directoryEntry) where T : new() => directoryEntry.NotNull(nameof(directoryEntry))
        .Properties
        .ToConfiguration()
        .Bind<T>()
        .NotNull($"Cannot bind to {nameof(T)}");
}
