using System.Collections.Frozen;
using System.Collections.Immutable;
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

        Proposal = new ProposalImpl(this, _graphClient, _logger);
        Search = new SearchImpl(this, _graphClient, _logger);
    }

    public ProposalImpl Proposal { get; }
    public SearchImpl Search { get; }

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

    public class ProposalImpl
    {
        private readonly TicketGroupClient _parent;
        private readonly IGraphClient _graphClient;
        private readonly ILogger _logger;

        internal ProposalImpl(TicketGroupClient parent, IGraphClient graphClient, ILogger logger)
        {
            _parent = parent;
            _graphClient = graphClient;
            _logger = logger;
        }

        public async Task<Option> Add(string ticketGroupId, ProposalRecord proposalRecord, ScopeContext context)
        {
            ticketGroupId.NotEmpty();
            if (!proposalRecord.Validate(out var r)) return r.LogStatus(context, nameof(ProposalRecord));

            var ticketGroupOption = await _parent.Get(ticketGroupId, context);
            if (ticketGroupOption.IsError()) return ticketGroupOption.ToOptionStatus();

            var ticketGroup = ticketGroupOption.Return();
            ticketGroup = ticketGroup with
            {
                Proposals = ticketGroup.Proposals.Values.Append(proposalRecord).ToFrozenDictionary(x => x.ProposalId, x => x),
            };

            var write = await _parent.Set(ticketGroup, context);
            return write;
        }

        public async Task<Option> Accept(string ticketGroupId, string proposalId, string principalId, ScopeContext context)
        {
            ticketGroupId.NotEmpty();
            proposalId.NotEmpty();
            principalId.NotEmpty();

            var response = await GetTicketGroup(ticketGroupId, proposalId, principalId, context);
            if (response.IsError()) return response.ToOptionStatus();

            (TicketGroupRecord ticketGroup, ProposalRecord proposalRecord, string? oldPrincipalId) = response.Return();

            proposalRecord = proposalRecord with
            {
                Accepted = new StateDetail { ByPrincipalId = principalId, Date = DateTime.UtcNow },
            };

            ticketGroup = ticketGroup with
            {
                Seats = ticketGroup.Seats
                    .Select(x => x switch
                    {
                        var v when v.SeatId == proposalRecord.SeatId && v.Date == proposalRecord.SeatDate => v with { AssignedToPrincipalId = principalId },
                        _ => x
                    }).ToImmutableArray(),

                Proposals = ticketGroup.Proposals
                    .ToDictionary(x => x.Key, x => x.Value)
                    .Action(x => x[proposalId] = proposalRecord)
                    .ToFrozenDictionary(),

                ChangeLogs = ticketGroup.ChangeLogs.Append(new ChangeLog
                {
                    ChangedByPrincipalId = principalId,
                    SeatId = proposalRecord.SeatId,
                    PropertyName = "AssignedToPrincipalId",
                    OldValue = oldPrincipalId,
                    NewValue = principalId
                }).ToImmutableArray(),
            };

            var write = await _parent.Set(ticketGroup, context);
            return write;
        }

        public async Task<Option> Reject(string ticketGroupId, string proposalId, string principalId, ScopeContext context)
        {
            ticketGroupId.NotEmpty();
            proposalId.NotEmpty();
            principalId.NotEmpty();

            var response = await GetTicketGroup(ticketGroupId, proposalId, principalId, context);
            if (response.IsError()) return response.ToOptionStatus();

            (TicketGroupRecord ticketGroup, ProposalRecord proposalRecord, string? oldPrincipalId) = response.Return();

            proposalRecord = proposalRecord with
            {
                Rejected = new StateDetail { ByPrincipalId = principalId, Date = DateTime.UtcNow },
            };

            ticketGroup = ticketGroup with
            {
                Proposals = ticketGroup.Proposals
                    .ToDictionary(x => x.Key, x => x.Value)
                    .Action(x => x[proposalId] = proposalRecord)
                    .ToFrozenDictionary(),

                ChangeLogs = ticketGroup.ChangeLogs.Append(new ChangeLog
                {
                    ChangedByPrincipalId = principalId,
                    SeatId = proposalRecord.SeatId,
                    PropertyName = "AssignedToPrincipalId-Rejected",
                    NewValue = principalId
                }).ToImmutableArray(),
            };

            var write = await _parent.Set(ticketGroup, context);
            return write;
        }

        private async Task<Option<(TicketGroupRecord ticketGroupRecord, ProposalRecord proposalRecord, string? oldPrincipalId)>> GetTicketGroup(
            string ticketGroupId,
            string proposalId,
            string principalId,
            ScopeContext context
            )
        {
            ticketGroupId.NotEmpty();
            proposalId.NotEmpty();
            principalId.NotEmpty();

            var ticketGroupOption = await _parent.Get(ticketGroupId, context);
            if (ticketGroupOption.IsError()) return ticketGroupOption.ToOptionStatus<(TicketGroupRecord, ProposalRecord, string?)>();

            var ticketGroup = ticketGroupOption.Return();

            if (!ticketGroup.CanAcceptProposal(principalId, context)) return StatusCode.Forbidden;

            if (!ticketGroup.Proposals.TryGetValue(proposalId, out var proposalRecord)) return StatusCode.NotFound;
            if (!proposalRecord.IsOpen())
            {
                context.LogError("ProposalId={proposalId} is not open", proposalId);
                return (StatusCode.Conflict, $"ProposalId={proposalId} is not open");
            }

            var oldSeats = ticketGroup.Seats
                .Where(x => x.SeatId == proposalRecord.SeatId && x.Date == proposalRecord.SeatDate)
                .ToArray();

            if (oldSeats.Length == 0) return (StatusCode.NotFound, $"Cannot find seatId={proposalRecord.SeatId}");
            if (oldSeats.Length > 1)
            {
                context.LogError("SeatId={seatId} has length={length}, should only be 1", proposalRecord.SeatId, oldSeats.Length);
                return (StatusCode.InternalServerError, $"SeatId={proposalRecord.SeatId} has length={oldSeats.Length}, should only be 1");
            }

            return (ticketGroup, proposalRecord, oldSeats[0].AssignedToPrincipalId);
        }
    }

    public class SearchImpl
    {
        private readonly TicketGroupClient _parent;
        private readonly IGraphClient _graphClient;
        private readonly ILogger _logger;

        internal SearchImpl(TicketGroupClient parent, IGraphClient graphClient, ILogger logger)
        {
            _parent = parent;
            _graphClient = graphClient;
            _logger = logger;
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

    }
}
