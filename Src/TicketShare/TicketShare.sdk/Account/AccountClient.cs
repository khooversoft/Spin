using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class AccountClient
{
    private const string _nodeTag = "account";
    private const string _edgeType = "account-owns";
    private readonly IGraphClient _graphClient;

    public AccountClient(IGraphClient graphClient)
    {
        _graphClient = graphClient.NotNull();
    }

    public Task<Option> Add(AccountRecord accountRecord, ScopeContext context) => AddOrSet(false, accountRecord, context);

    public async Task<Option> Delete(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();
        return await _graphClient.DeleteNode(ToAccountKey(principalId), context).ConfigureAwait(false);
    }

    public async Task<Option<AccountRecord>> Get(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();
        return await _graphClient.GetNode<AccountRecord>(ToAccountKey(principalId), context).ConfigureAwait(false);
    }

    public Task<Option> Set(AccountRecord accountRecord, ScopeContext context) => AddOrSet(true, accountRecord, context);

    private async Task<Option> AddOrSet(bool useSet, AccountRecord accountRecord, ScopeContext context)
    {
        var queryOption = AccountTool.CreateQuery(accountRecord, useSet, context);
        if (queryOption.IsError()) return queryOption.ToOptionStatus();

        string cmd = queryOption.Return();

        var result = await _graphClient.Execute(cmd, context).ConfigureAwait(false);
        if (result.IsError())
        {
            context.LogError("Failed to set account for principalId={principalId}", accountRecord.PrincipalId);
            return result.LogStatus(context, $"principalId={accountRecord.PrincipalId}").ToOptionStatus();
        }

        return result.ToOptionStatus();
    }

    public static string ToAccountKey(string principalId) => $"account:{principalId.NotEmpty().ToLowerInvariant()}";
}
