using System.Collections.Immutable;
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

    public async Task<string> GetPrincipalId()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        string principalId = authState.User.Identity.NotNull().Name.NotEmpty();
        return principalId;
    }

    public UserAccountContext GetContext() => new UserAccountContext(this, _logger);

    public async Task<Option<AccountRecord>> GetAccount()
    {
        var context = new ScopeContext(_logger);

        string principalId = await GetPrincipalId().ConfigureAwait(false);
        return await _accountClient.GetContext(principalId).Get(context).ConfigureAwait(false);
    }

    public async Task<Option> SetAccount(AccountRecord accountRecord)
    {
        accountRecord.NotNull();
        var context = new ScopeContext(_logger);
        var l = new List<string>();

        var result = await _accountClient.GetContext(accountRecord.PrincipalId)
            .Set(accountRecord, context)
            .ConfigureAwait(false);

        return result;
    }
}
