using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Identity;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class TicketGroupClient
{
    internal const string _nodeTag = "ticketGroup";
    internal const string _edgeType = "ticketGroup-user";
    private readonly IGraphClient _graphClient;
    private readonly ILogger<TicketGroupClient> _logger;
    private readonly HubChannelClient _hubChannelClient;

    public TicketGroupClient(IGraphClient graphClient, HubChannelClient hubChannelClient, IServiceProvider service, ILogger<TicketGroupClient> logger)
    {
        _graphClient = graphClient.NotNull();
        _hubChannelClient = hubChannelClient;
        _logger = logger.NotNull();

        Proposal = ActivatorUtilities.CreateInstance<TicketGroupProposalClient>(service, this);
        Search = ActivatorUtilities.CreateInstance<TicketGroupSearchClient>(service, this);
    }

    public TicketGroupProposalClient Proposal { get; }
    public TicketGroupSearchClient Search { get; }

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

        string[] removeTagList = [];
        var readOption = await Get(ticketGroupRecord.TicketGroupId, context).ConfigureAwait(false);
        if (readOption.IsOk())
        {
            var read = readOption.Return();
            if (read.ChannelId.IsNotEmpty()) ticketGroupRecord = ticketGroupRecord with { ChannelId = read.ChannelId };

            removeTagList = read.Roles
                .Select(x => x.PrincipalId)
                .Except(ticketGroupRecord.Roles.Select(x => x.PrincipalId), StringComparer.OrdinalIgnoreCase)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        if (ticketGroupRecord.ChannelId.IsEmpty())
        {
            ticketGroupRecord = ticketGroupRecord with { ChannelId = ToTicketGroupHubChannelId(ticketGroupRecord.TicketGroupId) };
        }

        string nodeKey = ToTicketGroupKey(ticketGroupRecord.TicketGroupId);

        if (!ticketGroupRecord.Validate(out var r)) return r.LogStatus(context, nameof(TicketGroupRecord));

        var roles = ticketGroupRecord.Roles
            .Select(x => x.PrincipalId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var cmd = new NodeCommandBuilder()
            .UseSet(useSet)
            .SetNodeKey(nodeKey)
            .AddTag(_nodeTag)
            .AddReferences(_edgeType, roles.Select(x => IdentityClient.ToUserKey(x)))
            .Action(x => removeTagList.ForEach(y => x.AddTag("-" + x)))
            .AddData("entity", ticketGroupRecord)
            .Build();

        var result = await _graphClient.Execute(cmd, context).ConfigureAwait(false);
        if (result.IsError())
        {
            context.LogError("Failed to set nodeKey={nodeKey}", nodeKey);
            return result.ToOptionStatus();
        }

        var hubChannelOption = await _hubChannelClient.CreateIfNotExist(ticketGroupRecord.ChannelId, nodeKey, context).ConfigureAwait(false);
        if (hubChannelOption.IsError()) return hubChannelOption;

        return result.ToOptionStatus();
    }

    public static string ToTicketGroupKey(string ticketGroupId) => $"ticketGroup:{ticketGroupId.NotEmpty().ToLowerInvariant()}";
    public static string ToTicketGroupHubChannelId(string ticketGroupId) => $"ticketGroup-channel/{ticketGroupId.NotEmpty().ToLowerInvariant()}";
}
