using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Identity;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk.Principals;

public class PrincipalManager
{
    private readonly IdentityClient _identityClient;
    private readonly AuthenticationAccess _authenticationAccess;
    private readonly ILogger<PrincipalManager> _logger;

    public PrincipalManager(IdentityClient identityClient, AuthenticationAccess authenticationAccess, ILogger<PrincipalManager> logger)
    {
        _identityClient = identityClient.NotNull();
        _authenticationAccess = authenticationAccess.NotNull();
        _logger = logger.NotNull();
    }

    //public async Task<Option<PrincipalIdentity>> GetPrincipalIdentity()
    //{
    //    string principalId = (await _authenticationAccess.GetPrincipalId()).NotNull();

    //    var result = await _identityClient.GetByPrincipalId(principalId, new ScopeContext(_logger));
    //}
}
