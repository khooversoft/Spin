using Microsoft.IdentityModel.Tokens;
using System;

namespace Toolbox.Security.Sign;

public interface IPrincipalSignature
{
    public string Kid { get; }

    public string Issuer { get; }

    public string? Audience { get; }

    public string? Subject { get; }

    string Sign(string payloadDigest);

    JwtTokenDetails? ValidateSignature(string jwt);

    public SigningCredentials GetSigningCredentials();

    public SecurityKey GetSecurityKey();
}
