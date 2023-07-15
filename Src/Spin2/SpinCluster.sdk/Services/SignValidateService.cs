using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Security.Jwt;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Services;

internal class SignValidateService : ISign, ISignValidate
{
    private readonly ILogger<SignValidateService> _logger;

    public SignValidateService(ILogger<SignValidateService> logger) => _logger = logger.NotNull();

    public Task<Option<string>> SignDigest(string kid, string messageDigest, ScopeContext context)
    {
        throw new NotImplementedException();
    }

    public Task<Option<JwtTokenDetails>> ValidateDigest(string jwtSignature, string messageDigest, ScopeContext context)
    {
        throw new NotImplementedException();
    }
}
