using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public interface IAccountClient
{
    Task<Option<AccountRecord>> Get(string principalId, ScopeContext context);
    Task<Option> Delete(string principalId, ScopeContext context);
    Task<Option> Set(AccountRecord accountName, ScopeContext context);
}

public class AccountClient : IAccountClient
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
        context = context.With(_logger);

        string cmd = GraphTool.DeleteNodeCommand(AccountRecordTool.ToAccountKey(principalId));

        var result = await _graphClient.ExecuteBatch(cmd, context);
        result.LogStatus(context, $"Deleting principalId={principalId}");
        return result.ToOptionStatus();
    }

    public async Task<Option<AccountRecord>> Get(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();
        context = context.With(_logger);

        var cmd = $"select (key={AccountRecordTool.ToAccountKey(principalId)}) return entity ;";
        var result = await _graphClient.Execute(cmd, context);
        if (result.IsError())
        {
            context.LogError("Failed to find principalId={principalId}", principalId);
            return result.LogStatus(context, $"principalId={principalId}").ToOptionStatus<AccountRecord>();
        }

        return result.Return().DataLinkToObject<AccountRecord>("entity");
    }

    public async Task<Option> Set(AccountRecord accountRecord, ScopeContext context)
    {
        context = context.With(_logger);
        if (!accountRecord.Validate(out var r)) return r.LogStatus(context, nameof(AccountRecord));

        string cmd = GraphTool.SetNodeCommand(AccountRecordTool.ToAccountKey(accountRecord.PrincipalId), base64: accountRecord.ToJson().ToBase64(), dataName: "entity");

        var result = await _graphClient.Execute(cmd, context);
        return result.ToOptionStatus();
    }
}
