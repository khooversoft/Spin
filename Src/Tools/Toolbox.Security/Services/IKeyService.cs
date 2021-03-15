using Microsoft.IdentityModel.Tokens;
using Toolbox.Security.Keys;

namespace Toolbox.Security.Services
{
    public interface IKeyService
    {
        CertificateCollection Certificates { get; }
        RsaPublicKeyCollection PublicKeys { get; }

        SecurityKey? GetSecurityKey(string kid);
        SigningCredentials? GetSigningCredentials(string kid);
    }
}