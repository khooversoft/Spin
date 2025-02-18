using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Toolbox.Graph.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Logging;

namespace TicketShare.sdk;

// Must be scoped
public class UserAccountContext
{
    private readonly IdentityClient _identityClient;
    private readonly AccountClient _accountClient;
    private readonly ILogger<UserAccountContext> _logger;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    private PrincipalIdentity? _principalIdentity;

    public UserAccountContext(
        IdentityClient identityClient,
        AccountClient accountClient,
        AuthenticationStateProvider authenticationStateProvider,
        ILogger<UserAccountContext> logger
        )
    {
        _identityClient = identityClient.NotNull();
        _accountClient = accountClient.NotNull();
        _authenticationStateProvider = authenticationStateProvider.NotNull();
        _logger = logger.NotNull();
    }

    public UserAccountEditContext GetEditContext() => new UserAccountEditContext(this, _logger);

    public void ClearCache() => _principalIdentity = null;

    public async Task<Option<PrincipalIdentity>> GetPrincipalIdentity(ScopeContext context, bool forceRefresh = false)
    {
        context = context.With(_logger);
        if (forceRefresh) _principalIdentity = null;

        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated == false) return (StatusCode.NotFound, "User is not authenticated");

        string principalId = authState.User.Identity.NotNull().Name.NotEmpty();
        if (_principalIdentity?.PrincipalId == principalId) return _principalIdentity;

        var result = await _identityClient.GetByPrincipalId(principalId, context).ConfigureAwait(false);
        return result;
    }

    public async Task<Option<AccountRecord>> GetAccount(ScopeContext context)
    {
        context = context.With(_logger);

        var identityOption = await GetPrincipalIdentity(context).ConfigureAwait(false);
        if (identityOption.IsError()) return identityOption.LogStatus(context, nameof(GetAccount)).ToOptionStatus<AccountRecord>();
        string principalId = identityOption.Return().PrincipalId;

        return await _accountClient
            .GetContext(principalId)
            .Get(context)
            .ConfigureAwait(false);
    }

    public async Task<Option> SetAccount(AccountRecord accountRecord, ScopeContext context)
    {
        accountRecord.NotNull();
        context = context.With(_logger);

        var result = await _accountClient
            .GetContext(accountRecord.PrincipalId)
            .Set(accountRecord, context)
            .ConfigureAwait(false);

        return result;
    }

    public async Task<bool> IsAccountEnabled(ScopeContext context)
    {
        context = context.With(_logger);

        var principalIdentity = await GetPrincipalIdentity(context).ConfigureAwait(false);
        if (principalIdentity.IsError()) return false;

        return principalIdentity.Return().EmailConfirmed;
    }
}
