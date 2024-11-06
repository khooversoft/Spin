using System.Collections.Frozen;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;


/// <summary>
/// Collection of tickets
/// </summary>
public sealed record TicketGroupRecord
{
    // Id = "ticketCollection:samTicket/2024/hockey
    public string TicketGroupId { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string OwnerPrincipalId { get; init; } = null!;
    public string? Tags { get; init; }
    public IReadOnlyList<RoleRecord> Roles { get; init; } = Array.Empty<RoleRecord>();
    public IReadOnlyList<SeatRecord> Seats { get; init; } = Array.Empty<SeatRecord>();
    public IReadOnlyList<ChangeLog> ChangeLogs { get; init; } = Array.Empty<ChangeLog>();
    public IReadOnlyDictionary<string, ProposalRecord> Proposals { get; init; } = FrozenDictionary<string, ProposalRecord>.Empty;

    public bool Equals(TicketGroupRecord? obj) => obj is TicketGroupRecord subject &&
        TicketGroupId == subject.TicketGroupId &&
        Name == subject.Name &&
        Description == obj.Description &&
        OwnerPrincipalId == obj.OwnerPrincipalId &&
        Tags == obj.Tags &&
        Enumerable.SequenceEqual(Roles, obj.Roles) &&
        Enumerable.SequenceEqual(Seats, obj.Seats) &&
        Enumerable.SequenceEqual(ChangeLogs, obj.ChangeLogs) &&
        Enumerable.SequenceEqual(Proposals.Values.OrderBy(x => x.ProposalId), subject.Proposals.Values.OrderBy(x => x.ProposalId));

    public override int GetHashCode() => HashCode.Combine(TicketGroupId, Name, Description, OwnerPrincipalId, Tags);

    public static IValidator<TicketGroupRecord> Validator { get; } = new Validator<TicketGroupRecord>()
        .RuleFor(x => x.TicketGroupId).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.OwnerPrincipalId).NotEmpty()
        .RuleForEach(x => x.Roles).Validate(RoleRecord.Validator)
        .RuleForEach(x => x.Seats).Validate(SeatRecord.Validator)
        .RuleForEach(x => x.ChangeLogs).Validate(ChangeLog.Validator)
        .RuleForEach(x => x.Proposals.Values).Validate(ProposalRecord.Validator)
        .Build();
}


public static class TicketCollectionRecordTool
{
    public static Option Validate(this TicketGroupRecord subject) => TicketGroupRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this TicketGroupRecord subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static string ToTicketGroupKey(string id) => $"ticketGroup:{id.NotEmpty().ToLowerInvariant()}";

    public static bool CanAcceptProposal(this TicketGroupRecord subject, string principalId, ScopeContext context)
    {
        subject.NotNull();
        principalId.NotEmpty();

        bool access = subject switch
        {
            var v when v.OwnerPrincipalId == principalId => true,
            var v when v.Roles.Any(x => x.PrincipalId == principalId && (x.MemberRole == RoleType.Owner || x.MemberRole == RoleType.Contributor)) => true,
            _ => false,
        };

        if (!access)
        {
            context.LogError(
                "PrincipalId={principalId} does not have access to accept proposals in ticketGroupId={ticketGroupId}",
                principalId,
                subject.TicketGroupId
                );
        }

        return access;
    }
}
