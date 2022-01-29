using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;

namespace Toolbox.Security.Sign;

public class PrincipalSignatureCertificate : PrincipalSignatureBase
{
    public PrincipalSignatureCertificate(string kid, string issuer, string? audience, X509Certificate2 x509Certificate2, string? subject = null)
        :base(kid, issuer, audience, subject)
    {
        Certificate = x509Certificate2;
    }

    public X509Certificate2 Certificate { get; }

    public override SigningCredentials GetSigningCredentials() => new SigningCredentials(new X509SecurityKey(Certificate), SecurityAlgorithms.RsaSha512);

    public override SecurityKey GetSecurityKey() => new X509SecurityKey(Certificate);
}
