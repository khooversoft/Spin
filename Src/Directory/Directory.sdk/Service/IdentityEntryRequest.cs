using Directory.sdk.Model;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Toolbox.Document;
using Toolbox.Tools;

namespace Directory.sdk.Service;

public record IdentityEntryRequest
{
    public string DirectoryId { get; init; } = null!;

    public string Issuer { get; init; } = null!;

    public string ClassType { get; init; } = ClassTypeName.Identity;

    public string Version { get; init; } = "v1";

    public IDictionary<string, EntryProperty> Properties { get; init; } = new Dictionary<string, EntryProperty>(StringComparer.OrdinalIgnoreCase);
}

public static class IdentityEntryRequestExtensions
{
    public static void Verify(this IdentityEntryRequest subject)
    {
        subject.VerifyNotNull(nameof(subject));
        subject.DirectoryId.VerifyDocumentId();
        subject.Issuer.VerifyNotEmpty(nameof(subject.Issuer));
        subject.ClassType.VerifyNotEmpty(nameof(subject.ClassType));
        subject.Properties.VerifyNotNull(nameof(subject.Properties));
    }

    public static IdentityEntry ToIdentityEntry(this IdentityEntryRequest subject)
    {
        RSA rsa = RSA.Create();

        return new IdentityEntry
        {
            DirectoryId = subject.DirectoryId,
            ClassType = "identity",
            Subject = subject.Issuer,
            PublicKey = rsa.ExportRSAPublicKey(),
            PrivateKey = rsa.ExportRSAPrivateKey(),
        };
    }
}