using System.Collections.Generic;
using Toolbox.Abstractions;
using Toolbox.Tools;

namespace Directory.sdk.Service;

public record IdentityEntryRequest
{
    public string DirectoryId { get; init; } = null!;

    public string Issuer { get; init; } = null!;

    public string ClassType { get; init; } = ClassTypeName.Identity;

    public string Version { get; init; } = "v1";

    public IList<string> Properties { get; init; } = new List<string>();

    //public IDictionary<string, EntryProperty> Properties { get; init; } = new Dictionary<string, EntryProperty>(StringComparer.OrdinalIgnoreCase);
}

public static class IdentityEntryRequestExtensions
{
    public static void Verify(this IdentityEntryRequest subject)
    {
        subject.NotNull();
        subject.DirectoryId.VerifyDocumentId();
        subject.Issuer.NotEmpty();
        subject.ClassType.NotEmpty();
        subject.Properties.NotNull();
    }

    //public static IdentityEntry ToIdentityEntry(this IdentityEntryRequest subject)
    //{
    //    RSA rsa = RSA.Create();

    //    return new IdentityEntry
    //    {
    //        DirectoryId = subject.DirectoryId,
    //        ClassType = "identity",
    //        Subject = subject.Issuer,
    //        PublicKey = rsa.ExportRSAPublicKey(),
    //        PrivateKey = rsa.ExportRSAPrivateKey(),
    //    };
    //}
}