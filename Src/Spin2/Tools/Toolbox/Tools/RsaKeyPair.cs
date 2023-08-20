using System.Security.Cryptography;

namespace Toolbox.Tools;

public readonly struct RsaKeyPair
{
    public RsaKeyPair(string keyId)
    {
        KeyId = keyId.NotEmpty();

        RSA rsa = RSA.Create();
        PublicKey = rsa.ExportRSAPublicKey();
        PrivateKey = rsa.ExportRSAPrivateKey();
    }

    public string KeyId { get; }
    public byte[] PublicKey { get; }
    public byte[] PrivateKey { get; }
}
