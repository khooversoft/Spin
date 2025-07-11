using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Graph.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class TicketGroupClient
{
    internal const string _nodeTag = "ticketGroup";
    internal const string _edgeType = "ticketGroup-user";
    private readonly IGraphClient _graphClient;
    private readonly ILogger<TicketGroupClient> _logger;

    public TicketGroupClient(IGraphClient graphClient, IServiceProvider service, ILogger<TicketGroupClient> logger)
    {
        _graphClient = graphClient.NotNull();
        _logger = logger.NotNull();
    }

    public Task<Option> Add(TicketGroupRecord ticketGroupRecord, ScopeContext context) => AddOrSet(false, ticketGroupRecord, context);

    public async Task<Option> Delete(string ticketGroupId, ScopeContext context)
    {
        ticketGroupId.NotEmpty();
        return await _graphClient.DeleteNode(ToTicketGroupKey(ticketGroupId), context).ConfigureAwait(false);
    }

    public async Task<Option<TicketGroupRecord>> Get(string ticketGroupId, ScopeContext context)
    {
        ticketGroupId.NotEmpty();
        return await _graphClient.GetNode<TicketGroupRecord>(ToTicketGroupKey(ticketGroupId), context).ConfigureAwait(false);
    }

    public Task<Option> Set(TicketGroupRecord ticketGroupRecord, ScopeContext context) => AddOrSet(true, ticketGroupRecord, context);

    private async Task<Option> AddOrSet(bool useSet, TicketGroupRecord ticketGroupRecord, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogTrace("AddOrSet TicketGroup, useSet={useSet}, ticketGroupId={ticketGroupId}", useSet, ticketGroupRecord.TicketGroupId);

        //string[] removeTagList = [];
        //var readOption = await Get(ticketGroupRecord.TicketGroupId, context).ConfigureAwait(false);
        //if (readOption.IsOk())
        //{
        //    var read = readOption.Return();
        //    if (read.ChannelId.IsNotEmpty()) ticketGroupRecord = ticketGroupRecord with { ChannelId = read.ChannelId };

        //    removeTagList = read.Roles
        //        .Select(x => x.PrincipalId)
        //        .Except(ticketGroupRecord.Roles.Select(x => x.PrincipalId), StringComparer.OrdinalIgnoreCase)
        //        .Distinct(StringComparer.OrdinalIgnoreCase)
        //        .ToArray();

        //    context.LogTrace("AddOrSet remove tags, ticketGroupId={ticketGroupId}, tags={tags}", ticketGroupRecord.TicketGroupId, removeTagList.Join(','));
        //}

        string securityGroupId = ticketGroupRecord.TicketGroupId;
        string channelId = ticketGroupRecord.TicketGroupId;

        SecurityGroupRecord securityGroup = SecurityGroupTool.CreateRecord(
            securityGroupId,
            $"Security group for ticket group {ticketGroupRecord.TicketGroupId}",
            ticketGroupRecord.Roles.Select(x => (user: x.PrincipalId, access: x.MemberRole.ToSecurityAccess()))
            );

        ChannelRecord channelRecord = ChannelTool.CreateRecord(channelId, securityGroupId, $"Channel for ticket group {ticketGroupRecord.TicketGroupId}");

        var cmdsOptions = new CommandBatchBuilder()
            .AddTicketGroup(ticketGroupRecord, useSet)
            .AddSecurityGroup(securityGroup, useSet)
            .AddChannel(channelRecord, useSet)
            .Build(context);
        if (cmdsOptions.IsError()) return cmdsOptions.ToOptionStatus();

        string cmds = cmdsOptions.Return();

        var result = await _graphClient.Execute(cmds, context).ConfigureAwait(false);
        if (result.IsError())
        {
            context.LogError("Failed to set ticketGroupId={ticketGroupId}", ticketGroupRecord.TicketGroupId);
            return result.ToOptionStatus();
        }

        return result.ToOptionStatus();
    }

    public async Task<Option<IReadOnlyList<TicketGroupRecord>>> Search(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();

        var cmd = new SelectCommandBuilder()
            .AddEdgeSearch(x => x.SetToKey(IdentityTool.ToNodeKey(principalId)).SetEdgeType(_edgeType))
            .AddRightJoin()
            .AddNodeSearch(x => x.AddTag(_nodeTag))
            .AddDataName("entity")
            .Build();

        var resultOption = await _graphClient.Execute(cmd, context).ConfigureAwait(false);
        if (resultOption.IsError()) resultOption.LogStatus(context, cmd).ToOptionStatus<IReadOnlyList<TicketGroupRecord>>();

        var list = resultOption.Return().DataLinkToObjects<TicketGroupRecord>("entity");
        return list.ToOption();
    }

    public static string ToTicketGroupKey(string ticketGroupId) => $"ticketGroup:{ticketGroupId.NotEmpty().ToLowerInvariant()}";
    public static string ToTicketGroupHubChannelId(string ticketGroupId) => $"ticketGroup-channel/{ticketGroupId.NotEmpty().ToLowerInvariant()}";
}
