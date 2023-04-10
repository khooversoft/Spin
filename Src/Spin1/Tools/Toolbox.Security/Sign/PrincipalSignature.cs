using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Cryptography;

namespace Toolbox.Security.Sign;

public class PrincipalSignature : PrincipalSignatureBase
{
    private RSA _rsa;

    public PrincipalSignature(string kid, string issuer, string? audience, string? subject = null, RSAParameters? rasParameters = null, DateTime? expires = null)
        : base(kid, issuer, audience, subject, expires)
    {
        _rsa = rasParameters switch
        {
            null => RSA.Create(),
            RSAParameters v => RSA.Create(v),
        };
    }

    public PrincipalSignature(string kid, string issuer, string? audience, string? subject, PrincipalSignature source, DateTime? expires = null)
        : base(kid, issuer, audience, subject, expires)
    {
        _rsa = source._rsa;
    }

    public RSAParameters RSAParameters(bool includePrivateKey) => _rsa.ExportParameters(includePrivateKey);

    public override SigningCredentials GetSigningCredentials() => new SigningCredentials(new RsaSecurityKey(RSAParameters(true)), SecurityAlgorithms.RsaSha512);

    public override SecurityKey GetSecurityKey() => new RsaSecurityKey(RSAParameters(false));
}
