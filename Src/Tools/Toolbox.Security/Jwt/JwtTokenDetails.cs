using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Toolbox.Tools;

namespace Toolbox.Security
{
    /// <summary>
    /// JWT token details (created when parsing a JWT token)
    /// </summary>
    public class JwtTokenDetails
    {
        public JwtTokenDetails(JwtSecurityToken jwtSecurityToken)
        {
            jwtSecurityToken.VerifyNotNull(nameof(jwtSecurityToken));

            JwtSecurityToken = jwtSecurityToken;

            NotBefore = ConvertTo(jwtSecurityToken?.Payload?.Nbf);
            ExpiresDate = ConvertTo(jwtSecurityToken?.Payload?.Exp);

            ApiKey = jwtSecurityToken!.Claims
                .Where(x => x.Type == JwtStandardClaimNames.WebKeyName)
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
            securityToken.VerifyNotNull(nameof(securityToken));
            claimsPrincipal.VerifyNotNull(nameof(claimsPrincipal));

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
        /// API Key (identification & authorization)
        /// </summary>
        public string? ApiKey { get; }

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

        /// <summary>
        /// Convert Unix time to date time
        /// </summary>
        /// <param name="value">Unix time</param>
        /// <returns></returns>
        private DateTime? ConvertTo(int? value)
        {
            if (value == null)
            {
                return null;
            }

            Verify.Assert(value > 0, "must be greater than 0");

            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds((int)value);
        }
    }
}