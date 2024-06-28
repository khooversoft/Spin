using System.Collections.Frozen;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using TicketShare.sdk;
using Toolbox.Extensions;
using Toolbox.Orleans;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShareWeb.Application;


public class AuthenticationConnector
{
    private readonly AccountConnector _accountConnector;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ILogger<AuthenticationConnector> _logger;
    private readonly IdentityConnector _identityConnector;

    public AuthenticationConnector(
        AuthenticationStateProvider authenticationStateProvider,
        AccountConnector accountConnector,
        IdentityConnector identityConnector,
        ILogger<AuthenticationConnector> logger
        )
    {
        _authenticationStateProvider = authenticationStateProvider.NotNull();
        _accountConnector = accountConnector.NotNull();
        _identityConnector = identityConnector.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<AccountRecord>> Get(ScopeContext context)
    {
        context = context.With(_logger);

        AuthenticationState authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        ClaimsPrincipal claimsPrincipal = authenticationState.User;

        string userId = claimsPrincipal.FindFirst(ClaimTypes.Name)!.Value.NotEmpty();
        string? email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;

        var principalIdentityOption = await _identityConnector.GetById(userId, context);
        if (principalIdentityOption.IsNotFound()) return createAccount(userId, userId, email);

        if (principalIdentityOption.IsError()) return principalIdentityOption.LogOnError(context, $"Cannot lookup userId={userId}").ToOptionStatus<AccountRecord>();
        var principalIdentity = principalIdentityOption.Return();

        var accountRecordOption = await _accountConnector.Get(userId, context);

        AccountRecord accountRecord = accountRecordOption switch
        {
            { StatusCode: StatusCode.OK } => accountRecordOption.Return(),

            _ => new AccountRecord
            {
                PrincipalId = userId,
                Name = principalIdentity.Name ?? userId,
                ContactItems = (principalIdentity.Email ?? email).ToNullIfEmpty() switch
                {
                    null => FrozenSet<ContactRecord>.Empty,
                    string v => new[] { new ContactRecord { Type = ContactType.Email, Value = v } }.ToFrozenSet(),
                },
            }
        };

        return accountRecord;

        static AccountRecord createAccount(string principalId, string? name, string? email) => new AccountRecord
        {
            PrincipalId = principalId,
            Name = name ?? principalId,
            ContactItems = email.ToNullIfEmpty() switch
            {
                null => FrozenSet<ContactRecord>.Empty,
                string v => new[] { new ContactRecord { Type = ContactType.Email, Value = v } }.ToFrozenSet(),
            },
        };
    }

    public async Task<Option> Set(AccountRecord accountRecord, ScopeContext context)
    {
        context = context.With(_logger);

        var option = await _accountConnector.Set(accountRecord, context);
        if (option.IsError()) return option.LogOnError(context, $"Cannot set accountRecord={accountRecord}");

        return option;
    }
}
