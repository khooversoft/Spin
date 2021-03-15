using System;
using System.Security.Cryptography;

namespace Toolbox.Security.Keys
{
    public class RsaPublicKey
    {
        private RSA _rsa;

        public RsaPublicKey(string? kid = null)
        {
            Kid = kid ?? Guid.NewGuid().ToString();

            _rsa = RSA.Create();
        }

        public RsaPublicKey(string kid, RSAParameters publicKey)
        {
            Kid = kid;
            _rsa = RSA.Create(publicKey);
        }

        public string Kid { get; }

        public RSAParameters PublicKey => _rsa.ExportParameters(includePrivateParameters: true);

        public RSA Rsa => _rsa;
    }
}