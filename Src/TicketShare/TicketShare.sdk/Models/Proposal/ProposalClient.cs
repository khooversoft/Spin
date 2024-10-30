using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Identity;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class ProposalClient
{
    private readonly IGraphClient _graphClient;
    private readonly ILogger<AccountClient> _logger;

    public ProposalClient(IGraphClient graphClient, ILogger<AccountClient> logger)
    {
        _graphClient = graphClient.NotNull();
        _logger = logger.NotNull();
    }

    public Task<Option> Add(ProposalRecord ticketGroupRecord, ScopeContext context) => AddOrSet(false, ticketGroupRecord, context);

    public async Task<Option> Delete(string ticketGroupId, ScopeContext context)
    {
        ticketGroupId.NotEmpty();
        return await _graphClient.DeleteNode(ToProposalKey(ticketGroupId), context);
    }

    public async Task<Option<ProposalRecord>> Get(string proposalId, ScopeContext context)
    {
        proposalId.NotEmpty();
        return await _graphClient.GetNode<ProposalRecord>(ToProposalKey(proposalId), context);
    }

    public async Task<Option<IReadOnlyList<ProposalRecord>>> GetByUser(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();

        var cmd = new SelectCommandBuilder()
            .AddEdgeSearch(x => x.SetFromKey(IdentityClient.ToUserKey(principalId)).SetEdgeType(NodeTypeTag))
            .Build();

        var result = await _graphClient.Execute(cmd, context);
        if (result.IsError()) return result.ToOptionStatus<IReadOnlyList<ProposalRecord>>();

        var list = result.Return()
            .DataLinkToObjects<ProposalRecord>("entity")
            .ToImmutableArray();

        return list;
    }

    public Task<Option> Set(ProposalRecord ticketGroupRecord, ScopeContext context) => AddOrSet(true, ticketGroupRecord, context);

    private async Task<Option> AddOrSet(bool useSet, ProposalRecord proposalRecord, ScopeContext context)
    {
        context = context.With(_logger);
        if (!proposalRecord.Validate(out var r)) return r.LogStatus(context, nameof(TicketGroupRecord));

        string nodeKey = ToProposalKey(proposalRecord.ProposalId);

        var seq = new Sequence<string>();

        seq += new NodeCommandBuilder()
            .UseSet(useSet)
            .SetNodeKey(nodeKey)
            .AddData("entity", proposalRecord)
            .Build();

        seq += new EdgeCommandBuilder()
            .UseSet()
            .SetFromKey(IdentityClient.ToUserKey(proposalRecord.AuthorPrincipalId))
            .SetToKey(nodeKey)
            .SetEdgeType("owns")
            .Build();

        seq += proposalRecord.Members.Select(x =>
        {
            return new EdgeCommandBuilder()
                .UseSet()
                .SetFromKey(IdentityClient.ToUserKey(x))
                .SetToKey(nodeKey)
                .SetEdgeType(NodeTypeTag)
                .Build();
        });

        string cmd = seq.Join(Environment.NewLine);

        var result = await _graphClient.Execute(cmd, context);
        if (result.IsError())
        {
            context.LogError("Failed to set nodeKey={nodeKey}", nodeKey);
            return result.LogStatus(context, $"nodeKey={nodeKey}").ToOptionStatus();
        }

        return result.ToOptionStatus();
    }

    private static string ToProposalKey(string proposalId) => $"proposal:{proposalId.NotEmpty().ToLowerInvariant()}";
    public static string NodeTypeTag { get; } = "proposal";
}
