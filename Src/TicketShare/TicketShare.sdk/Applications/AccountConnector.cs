using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using TicketShare.sdk.Actors;
using Toolbox.Extensions;
using Toolbox.Orleans;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class AccountConnector
{
    private readonly ILogger<AccountConnector> _logger;
    private readonly IClusterClient _clusterClient;
    private readonly IdentityActorConnector _identityConnector;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public AccountConnector(
        IClusterClient clusterClient, 
        AuthenticationStateProvider authenticationStateProvider, 
        IdentityActorConnector identityConnector, 
        ILogger<AccountConnector> logger)
    {
        _clusterClient = clusterClient.NotNull();
        _identityConnector = identityConnector.NotNull();
        _authenticationStateProvider = authenticationStateProvider.NotNull();
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

        var accountRecordOption = await Get(userId, context);

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

    public async Task<Option<AccountRecord>> Get(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();
        context = context.With(_logger);
        IAccountActor userActor = _clusterClient.GetUserActor();

        return await userActor.Get(principalId, context);
    }

    public async Task<Option> Set(AccountRecord accountRecord, ScopeContext context)
    {
        if( !accountRecord.Validate(out var r)) return r; 
        context = context.With(_logger);
        IAccountActor userActor = _clusterClient.GetUserActor();

        return await userActor.Set(accountRecord, context);
    }
}