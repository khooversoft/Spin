using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Security
{
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
        private readonly ILogger _logger = null!;

        public JwtTokenParser(IEnumerable<KeyValuePair<string, X509Certificate2>> certificates, IEnumerable<string> validIssuers, IEnumerable<string> validAudiences, ILogger<JwtTokenParser> logger)
        {
            certificates.VerifyNotNull(nameof(certificates));
            validIssuers.VerifyNotNull(nameof(validIssuers));
            validAudiences.VerifyNotNull(nameof(validAudiences));

            Certificates = certificates.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            ValidIssuers = new List<string>(validIssuers);
            ValidAudiences = new List<string>(validAudiences);
            _logger = logger;
        }

        public JwtTokenParser(IEnumerable<RsaPublicPrivateKey> rsaPublicPrivateKeys, IEnumerable<string> validIssuers, IEnumerable<string> validAudiences, ILogger<JwtTokenParser> logger)
        {
            validIssuers.VerifyNotNull(nameof(validIssuers));
            validAudiences.VerifyNotNull(nameof(validAudiences));

            RsaPublicPrivateKey = rsaPublicPrivateKeys.ToDictionary(x => x.Kid.ToString(), x => x, StringComparer.OrdinalIgnoreCase); ;
            ValidIssuers = new List<string>(validIssuers);
            ValidAudiences = new List<string>(validAudiences);
            _logger = logger;
        }

        /// <summary>
        /// List of X509 certificates that can be used to verify signature.  The field "KID" in the header specified
        /// which certificate thumbprint was used to create the signature.
        ///
        /// If JWT token does not specify a KID field, then the token is parsed and returned.  Not signature validation
        /// is performed.
        /// </summary>
        public IReadOnlyDictionary<string, X509Certificate2>? Certificates { get; } = null;

        /// <summary>
        /// RsaPublicPrivateKey
        /// </summary>
        public IReadOnlyDictionary<string, RsaPublicPrivateKey>? RsaPublicPrivateKey { get; set; } = null;

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
        public JwtTokenDetails? Parse(string token)
        {
            token.VerifyNotEmpty(nameof(token));

            try
            {
                SecurityKey privateKey;
                var tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(token);

                if ((jwtToken?.Header.Kid).IsEmpty()) return new JwtTokenDetails(jwtToken!);

                if (RsaPublicPrivateKey == null)
                {
                    if (!Certificates!.TryGetValue(jwtToken!.Header.Kid, out X509Certificate2? certificate)) return null;

                    privateKey = new X509SecurityKey(certificate);
                }
                else
                {
                    if (!RsaPublicPrivateKey!.TryGetValue(jwtToken!.Header.Kid, out RsaPublicPrivateKey? publicPrivateKey)) return null;
                    privateKey = new RsaSecurityKey(publicPrivateKey.GetPublicKey());
                }

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Parse JWT token failure");
                return null;
            }
        }

        /// <summary>
        /// Parse KID from JWT, extract from header
        /// </summary>
        /// <param name="token">token to parse</param>
        /// <returns>kid or null</returns>
        public static string? GetKidFromJwtToken(string? token)
        {
            if (token.IsEmpty()) return null;

            JwtSecurityToken jwtToken = new JwtSecurityTokenHandler()
                .ReadJwtToken(token);

            return jwtToken?.Header.Kid;
        }

        /// <summary>
        /// Parse Issuer from JWT, extract from header
        /// </summary>
        /// <param name="token">token to parse</param>
        /// <returns>issuer or null</returns>
        public static string? GetIssuerFromJwtToken(string? token)
        {
            if (token.IsEmpty()) return null;

            JwtSecurityToken jwtToken = new JwtSecurityTokenHandler()
                .ReadJwtToken(token);

            return jwtToken?.Issuer;
        }
    }
}