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
    internal const string _edgeType = "ticketGroup-own";
    internal const string _edgeTypeMember = "ticketGroup-member";
    private readonly IGraphClient _graphClient;
    private readonly ILogger<TicketGroupClient> _logger;

    public TicketGroupClient(IGraphClient graphClient, IServiceProvider service, ILogger<TicketGroupClient> logger)
    {
        _graphClient = graphClient.NotNull();
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
        return await _graphClient.DeleteNode(ToTicketGroupKey(ticketGroupId), context);
    }

    public async Task<Option<TicketGroupRecord>> Get(string ticketGroupId, ScopeContext context)
    {
        ticketGroupId.NotEmpty();
        return await _graphClient.GetNode<TicketGroupRecord>(ToTicketGroupKey(ticketGroupId), context);
    }

    public Task<Option> Set(TicketGroupRecord ticketGroupRecord, ScopeContext context) => AddOrSet(true, ticketGroupRecord, context);

    private async Task<Option> AddOrSet(bool useSet, TicketGroupRecord ticketGroupRecord, ScopeContext context)
    {
        context = context.With(_logger);
        if (!ticketGroupRecord.Validate(out var r)) return r.LogStatus(context, nameof(TicketGroupRecord));

        string[] removeTagList = [];

        var readOption = await Get(ticketGroupRecord.TicketGroupId, context);
        if (readOption.IsOk())
        {
            var read = readOption.Return();

            removeTagList = read.Roles
                .Select(x => x.PrincipalId)
                .Except(ticketGroupRecord.Roles.Select(x => x.PrincipalId), StringComparer.OrdinalIgnoreCase)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        string nodeKey = ToTicketGroupKey(ticketGroupRecord.TicketGroupId);

        var roles = ticketGroupRecord.Roles
            .Select(x => x.PrincipalId)
            .Append(ticketGroupRecord.OwnerPrincipalId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var cmd = new NodeCommandBuilder()
            .UseSet(useSet)
            .SetNodeKey(nodeKey)
            .AddTag(_nodeTag)
            .AddReference(_edgeType, IdentityClient.ToUserKey(ticketGroupRecord.OwnerPrincipalId))
            .AddReferences(_edgeTypeMember, roles.Select(x => IdentityClient.ToUserKey(x)))
            .Action(x => removeTagList.ForEach(y => x.AddTag("-" + x)))
            .AddData("entity", ticketGroupRecord)
            .Build();

        var result = await _graphClient.Execute(cmd, context);
        if (result.IsError())
        {
            context.LogError("Failed to set nodeKey={nodeKey}", nodeKey);
            return result.LogStatus(context, $"nodeKey={nodeKey}").ToOptionStatus();
        }

        return result.ToOptionStatus();
    }

    private static string ToTicketGroupKey(string ticketGroupId) => $"ticketGroup:{ticketGroupId.NotEmpty().ToLowerInvariant()}";
}
