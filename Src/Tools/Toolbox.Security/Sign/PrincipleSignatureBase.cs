using Microsoft.IdentityModel.Tokens;
using System;
using Toolbox.Tools;

namespace Toolbox.Security.Sign;

public abstract class PrincipleSignatureBase : IPrincipleSignature
{
    protected PrincipleSignatureBase(string kid, string issuer, string? audience, string? subject)
    {
        kid.VerifyNotEmpty(nameof(kid));
        issuer.VerifyNotEmpty(nameof(issuer));
        audience.VerifyNotEmpty(nameof(audience));

        Kid = kid;
        Issuer = issuer;
        Audience = audience;
        Subject = subject;
    }

    public string Kid { get; }

    public string Issuer { get; }

    public string? Audience { get; }

    public string? Subject { get; }

    public abstract SigningCredentials GetSigningCredentials();

    public abstract SecurityKey GetSecurityKey();

    public string Sign(string payloadDigest)
    {
        return new JwtTokenBuilder()
            .SetDigest(payloadDigest)
            .SetExpires(DateTime.Now.AddYears(10))
            .SetIssuedAt(DateTime.Now)
            .SetPrincipleSignature(this)
            .Build();
    }

    public JwtTokenDetails ValidateSignature(string jwt)
    {
        return new JwtTokenParserBuilder()
            .SetPrincipleSignature(this)
            .Build()
            .Parse(jwt);
    }
}
