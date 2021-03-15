using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using Toolbox.Security.Keys;

namespace Toolbox.Security.Services
{
    public class KeyService : IKeyService
    {
        public KeyService()
        {
            Certificates = new CertificateCollection();
            PublicKeys = new RsaPublicKeyCollection();
        }

        public KeyService(CertificateCollection certificates, RsaPublicKeyCollection publicKeys)
        {
            Certificates = new CertificateCollection(certificates);
            PublicKeys = new RsaPublicKeyCollection(publicKeys);
        }

        public CertificateCollection Certificates { get; }

        public RsaPublicKeyCollection PublicKeys { get; }

        public SecurityKey? GetSecurityKey(string kid)
        {
            if (PublicKeys.TryGetValue(kid, out RsaPublicKey? rsaPublicKey))
            {
                return new RsaSecurityKey(rsaPublicKey!.PublicKey);
            }

            if (Certificates.TryGetValue(kid, out X509Certificate2? certificate2))
            {
                return new X509SecurityKey(certificate2);
            }

            return null;
        }

        public SigningCredentials? GetSigningCredentials(string kid)
        {
            if (PublicKeys.TryGetValue(kid, out RsaPublicKey? rsaPublicKey))
            {
                var privateSecurityKey = new RsaSecurityKey(rsaPublicKey!.PublicKey);
                return new SigningCredentials(privateSecurityKey, SecurityAlgorithms.RsaSha512);
            }

            if (Certificates.TryGetValue(kid, out X509Certificate2? certificate2))
            {
                var securityKey = new X509SecurityKey(certificate2);
                return new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha512);
            }

            return null;
        }
    }
}