using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Security.Jwt;

/// <summary>
/// JWT token details (created when parsing a JWT token)
/// </summary>
public class JwtTokenDetails
{
    public JwtTokenDetails(JwtSecurityToken jwtSecurityToken)
    {
        jwtSecurityToken.NotNull();

        JwtSecurityToken = jwtSecurityToken;

        NotBefore = ConvertTo(jwtSecurityToken?.Payload?.NotBefore);
        ExpiresDate = ConvertTo(jwtSecurityToken?.Payload?.Expiration);

        Digest = jwtSecurityToken!.Claims
            .Where(x => x.Type == JwtStandardClaimNames.DigestName)
            .Select(x => x.Value)
            .FirstOrDefault();

        Subject = jwtSecurityToken.Claims
            .Where(x => x.Type == JwtStandardClaimNames.SubjectName)
            .Select(x => x.Value)
            .FirstOrDefault();

        Claims = jwtSecurityToken.Claims
            .Select(x => new Claim(x.Type, x.Value, x.ValueType, x.Issuer))
            .ToList();
    }

    public JwtTokenDetails(JwtSecurityToken jwtSecurityToken, SecurityToken securityToken, ClaimsPrincipal claimsPrincipal)
        : this(jwtSecurityToken)
    {
        securityToken.NotNull();
        claimsPrincipal.NotNull();

        SecurityToken = securityToken;
        ClaimsPrincipal = claimsPrincipal;
    }

    /// <summary>
    /// JWT security details
    /// </summary>
    public JwtSecurityToken JwtSecurityToken { get; }

    /// <summary>
    /// Security token created from JWT
    /// </summary>
    public SecurityToken? SecurityToken { get; }

    /// <summary>
    /// Identity (and authorization) created from JWT
    /// </summary>
    public ClaimsPrincipal? ClaimsPrincipal { get; }

    /// <summary>
    /// If specified, token should not be used before
    /// </summary>
    public DateTime? NotBefore { get; }

    /// <summary>
    /// If specified, token should not be used after
    /// </summary>
    public DateTime? ExpiresDate { get; }

    /// <summary>
    /// Digest claim
    /// </summary>
    public string? Digest { get; }

    /// <summary>
    /// Subject of JWT token
    /// </summary>
    public string? Subject { get; }

    /// <summary>
    /// Test for is expired, if expired date is not specified, true will be returned
    /// </summary>
    public bool IsExpired { get { return ExpiresDate == null || DateTime.UtcNow > ExpiresDate; } }

    /// <summary>
    /// Claims in JWT ticket
    /// </summary>
    public IReadOnlyList<Claim> Claims { get; }

    private DateTime? ConvertTo(long? value) => value == null ? null : new UnixDate((long)value).DateTime;
}