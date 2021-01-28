using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Security
{
    /// <summary>
    /// Build a JWT token parser, specify certificates, audiences, and issuers.  This is just a helper builder pattern class
    /// </summary>
    public class JwtTokenParserBuilder
    {
        public JwtTokenParserBuilder()
        {
        }

        public IDictionary<string, X509Certificate2> Certificates { get; } = new Dictionary<string, X509Certificate2>(StringComparer.OrdinalIgnoreCase);

        public IList<string> ValidIssuers { get; set; } = new List<string>();

        public IList<string> ValidAudiences { get; } = new List<string>();

        public JwtTokenParserBuilder Clear()
        {
            ValidIssuers.Clear();
            Certificates.Clear();
            ValidAudiences.Clear();

            return this;
        }

        public JwtTokenParserBuilder AddValidIssuer(params string[] validIssuer)
        {
            validIssuer.ForEach(x => ValidIssuers.Add(x));
            return this;
        }

        public JwtTokenParserBuilder AddCertificate(params X509Certificate2[] certificate)
        {
            certificate.VerifyNotNull(nameof(certificate));

            certificate.ForEach(x => Certificates.Add(x.Thumbprint, x));
            return this;
        }

        public JwtTokenParserBuilder AddValidAudience(params string[] validAudience)
        {
            validAudience.VerifyNotNull(nameof(validAudience));

            validAudience.ForEach(x => ValidAudiences.Add(x));
            return this;
        }

        public JwtTokenParser Build() => new JwtTokenParser(Certificates, ValidIssuers, ValidAudiences, new NullLogger<JwtTokenParser>());
    }
}