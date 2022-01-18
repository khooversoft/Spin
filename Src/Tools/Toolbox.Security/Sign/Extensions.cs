using System;

namespace Toolbox.Security.Sign;

public static class Extensions
{
    public static IPrincipleSignature WithAudience(this IPrincipleSignature principleSignature, string audience)
    {
        return principleSignature switch
        {
            PrincipleSignature v => new PrincipleSignature(v.Kid, v.Issuer, audience, v.Subject, v),

            PrincipleSignatureCertificate v => new PrincipleSignatureCertificate(v.Kid, v.Issuer, audience, v.Certificate, v.Subject),

            _ => throw new ArgumentException($"Unknown type={principleSignature.GetType().FullName}"),
        };
    }

    public static IPrincipleSignature WithIssuer(this IPrincipleSignature principleSignature, string issuer)
    {
        return principleSignature switch
        {
            PrincipleSignature v => new PrincipleSignature(v.Kid, issuer, v.Audience, v.Subject, v),

            PrincipleSignatureCertificate v => new PrincipleSignatureCertificate(v.Kid, issuer, v.Audience, v.Certificate, v.Subject),

            _ => throw new ArgumentException($"Unknown type={principleSignature.GetType().FullName}"),
        };
    }

    public static IPrincipleSignature WithSubject(this IPrincipleSignature principleSignature, string subject)
    {
        return principleSignature switch
        {
            PrincipleSignature v => new PrincipleSignature(v.Kid, v.Issuer, v.Audience, subject, v),

            PrincipleSignatureCertificate v => new PrincipleSignatureCertificate(v.Kid, v.Issuer, v.Audience, v.Certificate, subject),

            _ => throw new ArgumentException($"Unknown type={principleSignature.GetType().FullName}"),
        };
    }

}
