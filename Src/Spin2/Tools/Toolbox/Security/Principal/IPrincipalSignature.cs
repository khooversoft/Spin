using Microsoft.IdentityModel.Tokens;
using Toolbox.Security.Jwt;

namespace Toolbox.Security.Principal;

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