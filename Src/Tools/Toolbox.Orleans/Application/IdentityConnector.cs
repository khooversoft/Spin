using Microsoft.Extensions.Logging;
using Toolbox.Orleans;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

public class IdentityConnector
{
    private readonly ILogger<IdentityConnector> _logger;
    private readonly IClusterClient _clusterClient;

    public IdentityConnector(IClusterClient clusterClient, ILogger<IdentityConnector> logger)
    {
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Delete(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();
        context = context.With(_logger);
        return await _clusterClient.GetIdentityActor().Delete(principalId, context);
    }

    public async Task<Option<PrincipalIdentity>> GetById(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();
        context = context.With(_logger);
        return await _clusterClient.GetIdentityActor().GetById(principalId, context);
    }

    public async Task<Option<PrincipalIdentity>> GetByLogin(string loginProvider, string providerKey, ScopeContext context)
    {
        loginProvider.NotEmpty();
        providerKey.NotEmpty();
        context = context.With(_logger);
        return await _clusterClient.GetIdentityActor().GetByLogin(loginProvider, providerKey, context);
    }

    public async Task<Option<PrincipalIdentity>> GetByUserName(string userName, ScopeContext context)
    {
        userName.NotEmpty();
        context = context.With(_logger);
        return await _clusterClient.GetIdentityActor().GetByUserName(userName, context);
    }

    public async Task<Option<PrincipalIdentity>> GetByEmail(string email, ScopeContext context)
    {
        email.NotEmpty();
        context = context.With(_logger);
        return await _clusterClient.GetIdentityActor().GetByEmail(email, context);
    }

    public async Task<Option> Set(PrincipalIdentity user, ScopeContext context)
    {
        context.With(_logger);
        if (user.Validate().LogStatus(context, $"UserId={user.PrincipalId}").IsError(out Option v)) return v;

        return await _clusterClient.GetIdentityActor().Set(user, context);
    }
}
