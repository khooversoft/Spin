using Microsoft.IdentityModel.Tokens;
using Toolbox.Extensions;
using Toolbox.Security.Jwt;
using Toolbox.Security.Sign;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Security.Principal;

public abstract class PrincipalSignatureBase : IPrincipalSignature
{
    protected PrincipalSignatureBase(string kid, string issuer, string? audience, string? subject, DateTime? expires)
    {
        kid.NotEmpty();
        issuer.NotEmpty();
        audience.NotEmpty();

        Kid = kid;
        Issuer = issuer;
        Audience = audience;
        Subject = subject;
        Expires = expires ?? DateTime.UtcNow.AddYears(10);
    }

    public string Kid { get; }

    public string Issuer { get; }

    public string? Audience { get; }

    public string? Subject { get; }
    public DateTime Expires { get; }

    public abstract SigningCredentials GetSigningCredentials();

    public abstract SecurityKey GetSecurityKey();

    public Task<Option<string>> SignDigest(string kid, string messageDigest, string _)
    {
        kid.NotEmpty().Assert(x => x == Kid, "Kid does not match");
        messageDigest.NotEmpty();

        string jwt = new JwtTokenBuilder()
            .SetDigest(messageDigest)
            .SetExpires(DateTime.UtcNow.AddYears(10))
            .SetIssuedAt(DateTime.UtcNow)
            .SetPrincipleSignature(this)
            .Build();

        return jwt.ToOption().ToTaskResult();
    }

    public Task<Option> ValidateDigest(string jwtSignature, string messageDigest, string _)
    {
        jwtSignature.NotEmpty();
        messageDigest.NotEmpty();

        try
        {
            var details = new JwtTokenParserBuilder()
            .SetPrincipleSignature(this)
            .Build()
            .Parse(jwtSignature);

            if (details.JwtSecurityToken.Header.Kid.IsEmpty()) return new Option(StatusCode.BadRequest, "Missing kid in JWT")
                .ToTaskResult();

            details.JwtSecurityToken.Header.Assert(x => x.Kid == Kid, "Kid does not match");

            if (details.Digest.IsEmpty()) return new Option(StatusCode.BadRequest, "Missing Digest in JWT")
                .ToTaskResult();

            if (details.Digest != messageDigest) return new Option(StatusCode.BadRequest, "Message digest do not match")
                .ToTaskResult();

            return new Option(StatusCode.OK).ToTaskResult();
        }
        catch (Exception ex)
        {
            string msg = $"Jwt signature failed to verify, jwtSignature={jwtSignature}, ex={ex}";
            return new Option(StatusCode.BadRequest, msg).ToTaskResult();
        }
    }
}
