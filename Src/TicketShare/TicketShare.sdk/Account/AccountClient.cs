using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Graph.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class AccountClient
{
    private readonly IGraphClient _graphClient;
    public AccountClient(IGraphClient graphClient) => _graphClient = graphClient.NotNull();

    public AccountContext GetContext(string principalId) => new AccountContext(_graphClient, principalId);


    public async Task<Option<AccountRecord>> Create(string principalId, ScopeContext context)
    {
        principalId.NotNull();

        var identityLookup = await new IdentityClient(_graphClient).GetByPrincipalId(principalId, context).ConfigureAwait(false);
        if (identityLookup.IsError())
        {
            context.LogError("Cannot create user's account because principalId={principalId} not found", principalId);
            return identityLookup.ToOptionStatus<AccountRecord>();
        }

        var identityRecord = identityLookup.Return();

        AccountRecord accountRecord = new AccountRecord
        {
            PrincipalId = principalId,
            Name = identityRecord.Name,
            ContactItems = new[]
            {
                new ContactRecord { Type = ContactType.Email, Value = identityRecord.Email.NotEmpty() },
            }.ToImmutableArray(),
        };

        var setOption = await GetContext(accountRecord.PrincipalId).Set(accountRecord, context).ConfigureAwait(false);
        if (setOption.IsError()) return setOption.ToOptionStatus<AccountRecord>();

        return accountRecord;
    }
}
