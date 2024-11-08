using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Identity;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class AccountClient
{
    private readonly IGraphClient _graphClient;
    private readonly ILogger<AccountClient> _logger;

    public AccountClient(IGraphClient graphClient, IServiceProvider service, ILogger<AccountClient> logger)
    {
        _graphClient = graphClient.NotNull();
        _logger = logger.NotNull();

        Messages = ActivatorUtilities.CreateInstance<IdentityMessagesClient>(service, this);
    }

    public IdentityMessagesClient Messages { get; }

    public Task<Option> Add(AccountRecord accountRecord, ScopeContext context) => AddOrSet(false, accountRecord, context);

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

    public Task<Option> Set(AccountRecord accountRecord, ScopeContext context) => AddOrSet(true, accountRecord, context);

    private async Task<Option> AddOrSet(bool useSet, AccountRecord accountRecord, ScopeContext context)
    {
        context = context.With(_logger);
        if (!accountRecord.Validate(out var r)) return r.LogStatus(context, nameof(AccountRecord));

        string nodeKey = ToAccountKey(accountRecord.PrincipalId);

        var cmd = new NodeCommandBuilder()
            .UseSet(useSet)
            .SetNodeKey(nodeKey)
            .AddForeignKeyTag("owns", IdentityClient.ToUserKey(accountRecord.PrincipalId))
            .AddData("entity", accountRecord)
            .Build();

        var result = await _graphClient.Execute(cmd, context);
        if (result.IsError())
        {
            context.LogError("Failed to set nodeKey={nodeKey}", nodeKey);
            return result.LogStatus(context, $"nodeKey={nodeKey}").ToOptionStatus();
        }

        return result.ToOptionStatus();
    }

    private static string ToAccountKey(string principalId) => $"account:{principalId.NotEmpty().ToLowerInvariant()}";
}
