using System.Collections.Immutable;
using Microsoft.AspNetCore.Components.Authorization;
using TicketShare.sdk;
using Toolbox.Identity;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShareWeb.Application;

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

    public async Task<Option<AccountRecord>> GetAccount()
    {
        var context = new ScopeContext(_logger);

        string principalId = await GetPrincipalId().ConfigureAwait(false);
        Option<AccountRecord> accountRecordOption = await _accountClient.Get(principalId, context).ConfigureAwait(false);

        if (accountRecordOption.IsOk()) return accountRecordOption;

        Option<PrincipalIdentity> principalIdentityOption = await _identityClient.GetByPrincipalId(principalId, new ScopeContext(_logger)).ConfigureAwait(false);
        if (principalIdentityOption.IsError())
        {
            context.LogError("PrincipalId={principalId} not found", principalId);
            return StatusCode.NotFound;
        }

        var principalIdentity = principalIdentityOption.Return();

        AccountRecord accountRecord = new AccountRecord
        {
            PrincipalId = principalIdentity.PrincipalId.NotEmpty(),
            Name = principalIdentity.Name.NotEmpty(),
            ContactItems = new[]
            {
                new ContactRecord { Type = ContactType.Email, Value = principalIdentity.Email.NotEmpty() },
            }.ToImmutableArray(),
        };

        var writeOption = await _accountClient.Add(accountRecord, context).ConfigureAwait(false);
        if (writeOption.IsError())
        {
            context.LogError("PrincipalId={principalId} not found", principalId);
            return writeOption.ToOptionStatus<AccountRecord>();
        }

        return accountRecord;
    }

    public async Task<Option> SetAccount(AccountRecord accountRecord)
    {
        accountRecord.NotNull();
        var context = new ScopeContext(_logger);

        var result = await _accountClient.Set(accountRecord, context).ConfigureAwait(false);
        return result;
    }
}
