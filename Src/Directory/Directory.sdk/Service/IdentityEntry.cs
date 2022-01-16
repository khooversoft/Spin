using Azure;
using Directory.sdk.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Toolbox.Document;
using Toolbox.Tools;

namespace Directory.sdk.Service;

public record IdentityEntry
{
    public string DirectoryId { get; init; } = null!;

    public string Issuer { get; init; } = null!;

    public string ClassType { get; init; } = "identity";

    public string Version { get; init; } = "v1";

    public ETag? ETag { get; init; }

    public byte[] PublicKey {  get; init; } = null!;

    public byte[] PrivateKey { get; init; } = null!;

    public IDictionary<string, EntryProperty> Properties { get; init; } = new Dictionary<string, EntryProperty>(StringComparer.OrdinalIgnoreCase);
}

public static class IdentityEntryExtensions
{
    public static void Verify(this IdentityEntry subject)
    {
        subject.VerifyNotNull(nameof(subject));
        subject.DirectoryId.VerifyDocumentId();
        subject.Issuer.VerifyNotEmpty(nameof(subject.Issuer));
        subject.ClassType.VerifyNotEmpty(nameof(subject.ClassType));
        subject.PublicKey.VerifyNotNull(nameof(subject.PublicKey));
        subject.PrivateKey.VerifyNotNull(nameof(subject.PrivateKey));
        subject.Properties.VerifyNotNull(nameof(subject.Properties));
    }

    public static RSAParameters GetRsaParameters(this IdentityEntry identityEntry)
    {
        RSA rsa = RSA.Create();
        rsa.ImportRSAPublicKey(identityEntry.PublicKey, out int publicReadSize);
        rsa.ImportRSAPrivateKey(identityEntry.PrivateKey, out int privateReadSize);
        return rsa.ExportParameters(true);
    }
}
