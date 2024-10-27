using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class AccountClient
{
    private readonly IGraphClient _graphClient;
    private readonly ILogger<AccountClient> _logger;

    public AccountClient(IGraphClient graphClient, ILogger<AccountClient> logger)
    {
        _graphClient = graphClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Delete(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();
        return await _graphClient.DeleteNode(ToAccountKey(principalId), context);
    }

    public async Task<Option<AccountRecord>> Get(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();
        return await _graphClient.GetNode<AccountRecord>(ToAccountKey(principalId), context);
    }

    public async Task<Option<IReadOnlyList<AccountRecord>> GetAccounts(string principalId, ScopeContext context)
    {
    }

    public async Task<Option> Set(AccountRecord accountRecord, ScopeContext context)
    {
        context = context.With(_logger);
        if (!accountRecord.Validate(out var r)) return r.LogStatus(context, nameof(AccountRecord));

        string nodeKey = ToAccountKey(accountRecord.PrincipalId);
        string reserveTag = accountRecord.GetReserveTag();

        var cmd = new NodeCommandBuilder()
            .UseSet()
            .SetNodeKey(nodeKey)
            .AddTag(reserveTag)
            .AddData("entity", accountRecord)
            .AddIndex("reserve")
            .AddIndex("userName")
            .Build();

        var result = await _graphClient.Execute(cmd, context);
        if (result.IsError())
        {
            context.LogError("Failed to set nodeKey={nodeKey}", nodeKey);
            return result.LogStatus(context, $"nodeKey={nodeKey}").ToOptionStatus();
        }

        return result.ToOptionStatus();
    }

    private static string ToAccountKey(string id) => $"account:{id.NotEmpty().ToLower()}";
}
