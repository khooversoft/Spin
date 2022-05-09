using System;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Security.Sign;

public static class Extensions
{
    public static IPrincipalSignature WithAudience(this IPrincipalSignature principalSignature, string audience)
    {
        principalSignature.NotNull(nameof(principalSignature));

        return principalSignature switch
        {
            PrincipalSignature v => new PrincipalSignature(v.Kid, v.Issuer, audience, v.Subject, v),

            PrincipalSignatureCertificate v => new PrincipalSignatureCertificate(v.Kid, v.Issuer, audience, v.Certificate, v.Subject),

            _ => throw new ArgumentException($"Unknown type={principalSignature.GetType().FullName}"),
        };
    }

    public static IPrincipalSignature WithIssuer(this IPrincipalSignature principalSignature, string issuer)
    {
        principalSignature.NotNull(nameof(principalSignature));

        return principalSignature switch
        {
            PrincipalSignature v => new PrincipalSignature(v.Kid, issuer, v.Audience, v.Subject, v),

            PrincipalSignatureCertificate v => new PrincipalSignatureCertificate(v.Kid, issuer, v.Audience, v.Certificate, v.Subject),

            _ => throw new ArgumentException($"Unknown type={principalSignature.GetType().FullName}"),
        };
    }

    public static IPrincipalSignature WithSubject(this IPrincipalSignature principalSignature, string subject)
    {
        principalSignature.NotNull(nameof(principalSignature));

        return principalSignature switch
        {
            PrincipalSignature v => new PrincipalSignature(v.Kid, v.Issuer, v.Audience, subject, v),

            PrincipalSignatureCertificate v => new PrincipalSignatureCertificate(v.Kid, v.Issuer, v.Audience, v.Certificate, subject),

            _ => throw new ArgumentException($"Unknown type={principalSignature.GetType().FullName}"),
        };
    }

    public static Func<string, Task<string>> GetSign(this IPrincipalSignature principalSignature)
    {
        principalSignature.NotNull(nameof(principalSignature));

        return x => Task.FromResult(principalSignature.Sign(x));
    }
}
