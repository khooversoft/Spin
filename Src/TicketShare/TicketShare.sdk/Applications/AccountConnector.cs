using System.Collections.Immutable;
using System.Security.Claims;
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
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public AccountConnector(
        IClusterClient clusterClient,
        AuthenticationStateProvider authenticationStateProvider,
        ILogger<AccountConnector> logger)
    {
        _clusterClient = clusterClient.NotNull();
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

        var principalIdentityOption = await _clusterClient.GetIdentityActor().GetById(userId, context);
        if (principalIdentityOption.IsNotFound()) return createAccount(userId, userId, email);

        if (principalIdentityOption.IsError()) return principalIdentityOption.LogStatus(context, $"Cannot lookup userId={userId}").ToOptionStatus<AccountRecord>();
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
                    null => ImmutableArray<ContactRecord>.Empty,
                    string v => new[] { new ContactRecord { Type = ContactType.Email, Value = v } }.ToImmutableArray(),
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
                null => ImmutableArray<ContactRecord>.Empty,
                string v => new[] { new ContactRecord { Type = ContactType.Email, Value = v } }.ToImmutableArray(),
            },
        };
    }

    public Task<Option<AccountRecord>> Get(string principalId, ScopeContext context) => _clusterClient.GetUserActor().Get(principalId, context);

    public Task<Option> Set(AccountRecord accountRecord, ScopeContext context) => _clusterClient.GetUserActor().Set(accountRecord, context);
}