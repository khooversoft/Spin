using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace SpinCluster.sdk.Actors.Key;

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
