using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Toolbox.Extensions;
using Toolbox.Security.Services;
using Toolbox.Tools;

namespace Toolbox.Security
{
    /// <summary>
    /// Build a JWT token parser, specify certificates, audiences, and issuers.  This is just a helper builder pattern class
    /// </summary>
    public class JwtTokenParserBuilder
    {
        public IKeyService? KeyService { get; set; }

        public IList<string> ValidAudiences { get; } = new List<string>();

        public IList<string> ValidIssuers { get; set; } = new List<string>();

        public JwtTokenParserBuilder AddValidAudience(params string[] validAudience) => this.Action(x => validAudience.ForEach(y => ValidAudiences.Add(y)));

        public JwtTokenParserBuilder AddValidIssuer(params string[] validIssuer) => this.Action(x => validIssuer.ForEach(y => ValidIssuers.Add(y)));

        public JwtTokenParserBuilder SetKeyService(IKeyService keyService) => this.Action(x => x.KeyService = keyService);

        public JwtTokenParser Build()
        {
            KeyService.VerifyNotNull($"{nameof(KeyService)} is required");

            return new JwtTokenParser(KeyService, ValidIssuers, ValidAudiences);
        }
    }
}