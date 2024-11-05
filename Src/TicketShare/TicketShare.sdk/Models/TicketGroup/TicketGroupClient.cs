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
    private const string _nodeTag = "ticketGroup";
    private const string _edgeType = "ticketGroup-own";
    private readonly IGraphClient _graphClient;
    private readonly ILogger<AccountClient> _logger;

    public TicketGroupClient(IGraphClient graphClient, ILogger<AccountClient> logger)
    {
        _graphClient = graphClient.NotNull();
        _logger = logger.NotNull();
    }

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

    public async Task<Option<IReadOnlyList<TicketGroupRecord>>> GetByOwner(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();

        var cmd = new SelectCommandBuilder()
            .AddEdgeSearch(x => x.SetToKey(IdentityClient.ToUserKey(principalId)).SetEdgeType("owns"))
            .AddRightJoin()
            .AddNodeSearch(x => x.AddTag(_nodeTag))
            .AddDataName("entity")
            .Build();

        var resultOption = await _graphClient.Execute(cmd, context);
        if (resultOption.IsError()) resultOption.LogStatus(context, cmd).ToOptionStatus<IReadOnlyList<TicketGroupRecord>>();

        var list = resultOption.Return().DataLinkToObjects<TicketGroupRecord>("entity");
        return list.ToOption();
    }

    public async Task<Option<IReadOnlyList<TicketGroupRecord>>> GetByMember(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();

        var cmd = new SelectCommandBuilder()
            .AddEdgeSearch(x => x.SetToKey(IdentityClient.ToUserKey(principalId)).SetEdgeType(_edgeType))
            .AddRightJoin()
            .AddNodeSearch(x => x.AddTag(_nodeTag))
            .AddDataName("entity")
            .Build();

        var resultOption = await _graphClient.Execute(cmd, context);
        if (resultOption.IsError()) resultOption.LogStatus(context, cmd).ToOptionStatus<IReadOnlyList<TicketGroupRecord>>();

        var list = resultOption.Return().DataLinkToObjects<TicketGroupRecord>("entity");
        return list.ToOption();
    }

    public Task<Option> Set(TicketGroupRecord ticketGroupRecord, ScopeContext context) => AddOrSet(true, ticketGroupRecord, context);

    private async Task<Option> AddOrSet(bool useSet, TicketGroupRecord ticketGroupRecord, ScopeContext context)
    {
        context = context.With(_logger);
        if (!ticketGroupRecord.Validate(out var r)) return r.LogStatus(context, nameof(TicketGroupRecord));

        string nodeKey = ToTicketGroupKey(ticketGroupRecord.TicketGroupId);

        var roles = ticketGroupRecord.Roles
            .Select(x => x.PrincipalId)
            .Append(ticketGroupRecord.OwnerPrincipalId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var cmd = new NodeCommandBuilder()
            .UseSet(useSet)
            .SetNodeKey(nodeKey)
            .AddForeignKeyTag("owns", IdentityClient.ToUserKey(ticketGroupRecord.OwnerPrincipalId))
            .Action(x => roles.ForEach(y => x.AddForeignKeyTag(_edgeType, IdentityClient.ToUserKey(y))))
            .AddTag(_nodeTag)
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
