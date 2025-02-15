using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Toolbox.Graph.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class UserAccountManager
{
    private readonly IdentityClient _identityClient;
    private readonly AccountClient _accountClient;
    private readonly ILogger<UserAccountManager> _logger;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public UserAccountManager(
        IdentityClient identityClient,
        AccountClient accountClient,
        AuthenticationStateProvider authenticationStateProvider,
        ILogger<UserAccountManager> logger
        )
    {
        _identityClient = identityClient.NotNull();
        _accountClient = accountClient.NotNull();
        _authenticationStateProvider = authenticationStateProvider.NotNull();
        _logger = logger.NotNull();
    }

    public UserAccountContext GetContext() => new UserAccountContext(this, _logger);

    public async Task<string> GetPrincipalId(ScopeContext _)
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        string principalId = authState.User.Identity.NotNull().Name.NotEmpty();
        return principalId;
    }

    public async Task<Option<PrincipalIdentity>> GetPrincipalIdentity(ScopeContext context)
    {
        context = context.With(_logger);

        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated == false) return (StatusCode.NotFound, "User is not authenticated");

        string principalId = authState.User.Identity.NotNull().Name.NotEmpty();
        var result = await _identityClient.GetByPrincipalId(principalId, context).ConfigureAwait(false);

        return result;
    }

    public async Task<Option<AccountRecord>> GetAccount(ScopeContext context)
    {
        context = context.With(_logger);

        string principalId = await GetPrincipalId(context).ConfigureAwait(false);
        return await _accountClient.GetContext(principalId).Get(context).ConfigureAwait(false);
    }

    public async Task<Option> SetAccount(AccountRecord accountRecord, ScopeContext context)
    {
        accountRecord.NotNull();
        context = context.With(_logger);

        var result = await _accountClient.GetContext(accountRecord.PrincipalId)
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
