using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;

namespace Toolbox.Security.Principal;

public class PrincipalSignatureCertificate : PrincipalSignatureBase
{
    public PrincipalSignatureCertificate(string kid, string issuer, string? audience, X509Certificate2 x509Certificate2, string? subject = null, DateTime? expires = null)
        : base(kid, issuer, audience, subject, expires)
    {
        Certificate = x509Certificate2;
    }

    public X509Certificate2 Certificate { get; }

    public override SigningCredentials GetSigningCredentials() => new SigningCredentials(new X509SecurityKey(Certificate), SecurityAlgorithms.RsaSha512);

    public override SecurityKey GetSecurityKey() => new X509SecurityKey(Certificate);
}
