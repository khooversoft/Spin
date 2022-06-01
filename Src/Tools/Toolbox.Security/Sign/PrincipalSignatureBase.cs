using Microsoft.IdentityModel.Tokens;
using System;
using Toolbox.Tools;

namespace Toolbox.Security.Sign;

public abstract class PrincipalSignatureBase : IPrincipalSignature
{
    protected PrincipalSignatureBase(string kid, string issuer, string? audience, string? subject)
    {
        kid.NotEmpty();
        issuer.NotEmpty();
        audience.NotEmpty();

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

    public string Sign(string payloadDigest) => new JwtTokenBuilder()
            .SetDigest(payloadDigest)
            .SetExpires(DateTime.UtcNow.AddYears(10))
            .SetIssuedAt(DateTime.UtcNow)
            .SetPrincipleSignature(this)
            .Build();

    public JwtTokenDetails ValidateSignature(string jwt) => new JwtTokenParserBuilder()
            .SetPrincipleSignature(this)
            .Build()
            .Parse(jwt);
}
