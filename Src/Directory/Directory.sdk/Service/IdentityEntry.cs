﻿using Azure;
using System.Collections.Generic;
using System.Security.Cryptography;
using Toolbox.Abstractions;
using Toolbox.Tools;

namespace Directory.sdk.Service;

public record IdentityEntry
{
    public string DirectoryId { get; init; } = null!;

    public string Subject { get; init; } = null!;

    public string ClassType { get; init; } = ClassTypeName.Identity;

    public string Version { get; init; } = "v1";

    public ETag? ETag { get; init; }

    public byte[] PublicKey { get; init; } = null!;

    public byte[]? PrivateKey { get; init; }

    public IList<string> Properties { get; init; } = new List<string>();
}


public static class IdentityEntryExtensions
{
    public static void Verify(this IdentityEntry subject)
    {
        subject.VerifyNotNull(nameof(subject));
        subject.DirectoryId.VerifyDocumentId();
        subject.Subject.VerifyNotEmpty(nameof(subject.Subject));
        subject.ClassType.VerifyNotEmpty(nameof(subject.ClassType));
        subject.PublicKey.VerifyNotNull(nameof(subject.PublicKey));
        subject.Properties.VerifyNotNull(nameof(subject.Properties));
    }

    public static void VerifyWithPrivateKey(this IdentityEntry subject)
    {
        subject.Verify();
        subject.PrivateKey.VerifyNotNull(nameof(subject.PrivateKey));
    }

    public static RSAParameters GetRsaParameters(this IdentityEntry identityEntry)
    {
        identityEntry.VerifyNotNull(nameof(identityEntry));
        identityEntry.PublicKey.VerifyNotNull(nameof(identityEntry.PublicKey));

        RSA rsa = RSA.Create();
        rsa.ImportRSAPublicKey(identityEntry.PublicKey, out int publicReadSize);

        if (identityEntry.PrivateKey != null)
        {
            rsa.ImportRSAPrivateKey(identityEntry.PrivateKey, out int privateReadSize);
        }

        return rsa.ExportParameters(identityEntry.PrivateKey != null);
    }
}
