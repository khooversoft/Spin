using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Tools;

namespace Toolbox.Security.Jwt;

/// <summary>
/// JWT Token parser - designed to parse and validate many tokens based on the same set of issuers
/// and same set of audiences
///
/// Parse a token and does validation based on issuers and audiences.
///
/// Search a collection of certificates for thumbprint that matches key id (KID) in the JWT token's header
/// </summary>
public class JwtTokenParser
{
    private readonly IPrincipalSignature _principleSignature;

    public JwtTokenParser(IPrincipalSignature principleSignature, IEnumerable<string?> validIssuers, IEnumerable<string?> validAudiences)
    {
        principleSignature.NotNull();
        validIssuers.NotNull();
        validAudiences.NotNull();

        ValidIssuers = validIssuers
            .Append(principleSignature.Issuer)
            .Where(x => !x.IsEmpty())
            .Select(x => x!)
            .ToList();

        ValidAudiences = validAudiences
            .Append(principleSignature.Audience)
            .Where(x => !x.IsEmpty())
            .Select(x => x!).ToList();

        _principleSignature = principleSignature;
    }

    /// <summary>
    /// List valid JWT issuers (can be empty list).  If specified, will be used to verify JWT.
    /// </summary>
    public IReadOnlyList<string> ValidIssuers { get; }

    /// <summary>
    /// List of valid audiences (can be empty list).  If specified, will be used to verify JWT
    /// </summary>
    public IReadOnlyList<string> ValidAudiences { get; }

    /// <summary>
    /// Parse JWT token to details
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="token">JWT token</param>
    /// <returns>token details or null</returns>
    public JwtTokenDetails Parse(string token)
    {
        token.NotEmpty();

        var tokenHandler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(token);

        if ((jwtToken?.Header.Kid).IsEmpty()) return new JwtTokenDetails(jwtToken!);

        SecurityKey privateKey = _principleSignature.GetSecurityKey().NotNull();

        var validation = new TokenValidationParameters
        {
            RequireExpirationTime = true,
            ValidateLifetime = true,

            ValidateIssuer = ValidIssuers.Count > 0,
            ValidIssuers = ValidIssuers.Count > 0 ? ValidIssuers : null,
            ValidateAudience = ValidAudiences.Count > 0,
            ValidAudiences = ValidAudiences.Count > 0 ? ValidAudiences : null,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = privateKey,
        };

        ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(token, validation, out SecurityToken securityToken);
        return new JwtTokenDetails(jwtToken, securityToken, claimsPrincipal);
    }

    /// <summary>
    /// Parse KID from JWT, extract from header
    /// </summary>
    /// <param name="token">token to parse</param>
    /// <returns>kid or null</returns>
    public static string? GetKidFromJwtToken(string? token)
    {
        if (token.IsEmpty()) return null;

        JwtSecurityToken jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        return jwtToken.Header?.Kid;
    }

    /// <summary>
    /// Parse Issuer from JWT, extract from header
    /// </summary>
    /// <param name="token">token to parse</param>
    /// <returns>issuer or null</returns>
    public static string? GetIssuerFromJwtToken(string? token)
    {
        if (token.IsEmpty()) return null;

        JwtSecurityToken jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        return jwtToken.Issuer;
    }
}