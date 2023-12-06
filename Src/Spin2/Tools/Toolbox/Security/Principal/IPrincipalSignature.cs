using Microsoft.IdentityModel.Tokens;

namespace Toolbox.Security;

public interface IPrincipalSignature : ISign, ISignValidate
{
    public string Kid { get; }
    public string Issuer { get; }
    public string? Audience { get; }
    public string? Subject { get; }

    public SigningCredentials GetSigningCredentials();
    public SecurityKey GetSecurityKey();
}