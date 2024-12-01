using System.Collections.Immutable;
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

    public UserAccountManager(IdentityClient identityClient, AccountClient accountClient, ILogger<UserAccountManager> logger)
    {
        _identityClient = identityClient.NotNull();
        _accountClient = accountClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<AccountRecord>> GetAccount(string principalId)
    {
        principalId.NotEmpty();
        var context = new ScopeContext(_logger);

        Option<AccountRecord> accountRecordOption = await _accountClient.Get(principalId, context);
        if (accountRecordOption.IsOk()) return accountRecordOption;

        Option<PrincipalIdentity> principalIdentityOption = await _identityClient.GetByPrincipalId(principalId, new ScopeContext(_logger));
        if (principalIdentityOption.IsError()) return StatusCode.NotFound;
        var principalIdentity = principalIdentityOption.Return();

        AccountRecord accountRecord = new AccountRecord
        {
            PrincipalId = principalIdentity.PrincipalId,
            Name = principalIdentity.Name,
            ContactItems = new[]
            {
                new ContactRecord { Type = ContactType.Email, Value = principalIdentity.Email },
            }.ToImmutableArray(),
        };

        var writeOption = await _accountClient.Add(accountRecord, context);
        if (writeOption.IsError()) return writeOption.ToOptionStatus<AccountRecord>();

        return accountRecord;
    }

    public async Task<Option> SetAccount(AccountRecord accountRecord)
    {
        accountRecord.NotNull();
        var context = new ScopeContext(_logger);

        var result = await _accountClient.Set(accountRecord, context);
        return result;
    }
}
