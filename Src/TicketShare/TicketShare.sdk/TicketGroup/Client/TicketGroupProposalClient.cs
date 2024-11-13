using System.Collections.Frozen;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class TicketGroupProposalClient
{
    private readonly TicketGroupClient _ticketGroupClient;
    private readonly ILogger<TicketGroupProposalClient> _logger;

    public TicketGroupProposalClient(TicketGroupClient ticketGroupClient, ILogger<TicketGroupProposalClient> logger)
    {
        _ticketGroupClient = ticketGroupClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Add(string ticketGroupId, ProposalRecord proposalRecord, ScopeContext context)
    {
        ticketGroupId.NotEmpty();
        if (!proposalRecord.Validate(out var r)) return r.LogStatus(context, nameof(ProposalRecord));

        var ticketGroupOption = await _ticketGroupClient.Get(ticketGroupId, context);
        if (ticketGroupOption.IsError()) return ticketGroupOption.ToOptionStatus();

        var ticketGroup = ticketGroupOption.Return();
        ticketGroup = ticketGroup with
        {
            Proposals = ticketGroup.Proposals.Values.Append(proposalRecord).ToFrozenDictionary(x => x.ProposalId, x => x),
        };

        var write = await _ticketGroupClient.Set(ticketGroup, context);
        if (write.IsError()) return write;

        var message = await SendMessage(ticketGroup, proposalRecord, "Propose seat change", context);
        return message;
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

        var write = await _ticketGroupClient.Set(ticketGroup, context);
        if (write.IsError()) return write;

        var message = await SendMessage(ticketGroup, proposalRecord, "Propose seat change accepted", context);
        return message;
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

        var write = await _ticketGroupClient.Set(ticketGroup, context);
        if (write.IsError()) return write;

        var message = await SendMessage(ticketGroup, proposalRecord, "Propose seat change rejected", context);
        return message;
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

        var ticketGroupOption = await _ticketGroupClient.Get(ticketGroupId, context);
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

    private async Task<Option> SendMessage(TicketGroupRecord ticketGroup, ProposalRecord proposal, string message, ScopeContext context)
    {
        var currentOwner = ticketGroup.Seats
            .Where(x => x.SeatId == proposal.SeatId && x.Date == proposal.SeatDate)
            .Select(x => x.AssignedToPrincipalId)
            .FirstOrDefault();

        if (currentOwner == null) return StatusCode.OK;

        //var messageOption = await _accountClient.Messages.Send(proposal.Proposed.ByPrincipalId, currentOwner, message, proposal.ProposalId, context);
        //return messageOption;
        return default;
    }
}
