using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Graph.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class TicketMasterClient
{
    internal const string _nodeTag = "ticketGroup";
    internal const string _edgeType = "ticketGroup-user";
    private readonly IGraphClient _graphClient;
    private readonly ILogger<TicketGroupClient> _logger;

    public TicketMasterClient(IGraphClient graphClient, IServiceProvider service, ILogger<TicketGroupClient> logger)
    {
        _graphClient = graphClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Delete(string ticketMasterId, ScopeContext context)
    {
        ticketMasterId.NotEmpty();
        return await _graphClient.DeleteNode(TicketMasterRecordTool.ToNodeKey(ticketMasterId), context).ConfigureAwait(false);
    }

    public async Task<Option<TicketMasterRecord>> Get(string ticketMasterId, ScopeContext context)
    {
        ticketMasterId.NotEmpty();
        return await _graphClient.GetNode<TicketMasterRecord>(TicketMasterRecordTool.ToNodeKey(ticketMasterId), context).ConfigureAwait(false);
    }

    public async Task<Option> Set(TicketMasterRecord ticketMasterRecord, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogTrace("Set TicketMaster configuraiton");

        var queryOption = ticketMasterRecord.CreateQuery(context);
        if (ticketMasterRecord.Validate().IsError(out var r)) return r.LogStatus(context, nameof(TicketMasterRecord));

        string cmds = queryOption.Return();

        var result = await _graphClient.Execute(cmds, context).ConfigureAwait(false);
        if (result.IsError())
        {
            context.LogError("Failed to set ticketGroupId={ticketGroupId}", ticketMasterRecord.TicketMasterId);
            return result.ToOptionStatus();
        }

        return result.ToOptionStatus();
    }
}
