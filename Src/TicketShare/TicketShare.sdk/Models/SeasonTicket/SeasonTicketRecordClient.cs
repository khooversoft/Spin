using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk.Models.SeasonTicket;

public interface ISeasonTicketRecordClient
{
    Task<Option<AccountRecord>> Get(string seasonTicketId, ScopeContext context);
    Task<Option> Delete(string seasonTicketId, ScopeContext context);
    Task<Option> Set(AccountRecord accountName, ScopeContext context);
}

public class SeasonTicketRecordClient : ISeasonTicketRecordClient
{
    private readonly IGraphClient _graphClient;
    private readonly ILogger<SeasonTicketRecordClient> _logger;

    public SeasonTicketRecordClient(IGraphClient graphClient, ILogger<SeasonTicketRecordClient> logger)
    {
        _graphClient = graphClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Delete(string seasonTicketId, ScopeContext context)
    {
        seasonTicketId.NotEmpty();
        context = context.With(_logger);

        string cmd = GraphTool.DeleteNodeCommand(SeasonTicketRecordTool.ToSeasonTicketKey(seasonTicketId));

        var result = await _graphClient.ExecuteBatch(cmd, context);
        result.LogStatus(context, $"Deleting principalId={seasonTicketId}");
        return result.ToOptionStatus();
    }

    public async Task<Option<AccountRecord>> Get(string seasonTicketId, ScopeContext context)
    {
        seasonTicketId.NotEmpty();
        context = context.With(_logger);

        var cmd = $"select (key={SeasonTicketRecordTool.ToSeasonTicketKey(seasonTicketId)}) return entity ;";
        var result = await _graphClient.Execute(cmd, context);
        if (result.IsError())
        {
            context.LogError("Failed to find principalId={principalId}", seasonTicketId);
            return result.LogStatus(context, $"principalId={seasonTicketId}").ToOptionStatus<AccountRecord>();
        }

        return result.Return().DataLinkToObject<AccountRecord>("entity");
    }

    public async Task<Option> Set(AccountRecord seasonTicketId, ScopeContext context)
    {
        context = context.With(_logger);
        if (!seasonTicketId.Validate(out var r)) return r.LogStatus(context, nameof(AccountRecord));

        string base64 = seasonTicketId.ToJson().ToBase64();
        //string cmd = GraphTool.SetNodeCommand(AccountRecordTool.ToAccountKey(seasonTicketId.PrincipalId), base64: base64, dataName: "entity");

        var result = await _graphClient.Execute("", context);
        return result.ToOptionStatus();
    }
}
