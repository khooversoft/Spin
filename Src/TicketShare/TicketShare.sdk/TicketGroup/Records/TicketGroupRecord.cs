using System.Collections.Frozen;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;


/// <summary>
/// Collection of tickets
/// </summary>
public sealed record TicketGroupRecord
{
    // Id = "ticketGroup:samTicket/2024/hockey
    // Channel = "hub-channel:ticketGroup/samTicket/2024/hockey
    public string TicketGroupId { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string ChannelId { get; init; } = null!;
    public IReadOnlyList<RoleRecord> Roles { get; init; } = Array.Empty<RoleRecord>();
    public IReadOnlyList<SeatRecord> Seats { get; init; } = Array.Empty<SeatRecord>();
    public IReadOnlyList<ChangeLog> ChangeLogs { get; init; } = Array.Empty<ChangeLog>();
    public IReadOnlyDictionary<string, ProposalRecord> Proposals { get; init; } = FrozenDictionary<string, ProposalRecord>.Empty;

    public bool Equals(TicketGroupRecord? obj)
    {
        var result = obj is TicketGroupRecord subject &&
            TicketGroupId == subject.TicketGroupId &&
            Name == subject.Name &&
            Description == obj.Description &&
            ChannelId == obj.ChannelId &&
            Enumerable.SequenceEqual(Roles, obj.Roles) &&
            Enumerable.SequenceEqual(Seats, obj.Seats) &&
            Enumerable.SequenceEqual(ChangeLogs, obj.ChangeLogs) &&
            Enumerable.SequenceEqual(Proposals.Values.OrderBy(x => x.ProposalId), subject.Proposals.Values.OrderBy(x => x.ProposalId));

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(TicketGroupId, Name, Description, ChannelId, Roles, Seats, ChangeLogs, Proposals);

    public static IValidator<TicketGroupRecord> Validator { get; } = new Validator<TicketGroupRecord>()
        .RuleFor(x => x.TicketGroupId).Must(TicketGroupRecordTool.ValidateTicketGroupId)
        .RuleFor(x => x.Name).Must(TicketGroupRecordTool.ValidateName)
        .RuleFor(x => x.Description).Must(TicketGroupRecordTool.ValidateDescription)
        .RuleFor(x => x.ChannelId).Must(StandardValidation.IsName, _ => StandardValidation.NameError)
        .RuleForEach(x => x.Roles).Validate(RoleRecord.Validator)
        .RuleForEach(x => x.Seats).Validate(SeatRecord.Validator)
        .RuleForEach(x => x.ChangeLogs).Validate(ChangeLog.Validator)
        .RuleForEach(x => x.Proposals.Values).Validate(ProposalRecord.Validator)
        .Build();
}


public static class TicketGroupRecordTool
{
    public static Option ValidateTicketGroupId(string value) => StandardValidation.IsName(value) switch
    {
        true => StatusCode.OK,
        false => (StatusCode.BadRequest, StandardValidation.NameError),
    };

    public static Option ValidateName(string value) => StandardValidation.IsName(value) switch
    {
        true => StatusCode.OK,
        false => (StatusCode.BadRequest, StandardValidation.NameError),
    };

    public static Option ValidateDescription(string? value) => (value.IsEmpty() || StandardValidation.IsDescrption(value)) switch
    {
        true => StatusCode.OK,
        false => (StatusCode.BadRequest, StandardValidation.DescriptionError),
    };

    public static Option Validate(this TicketGroupRecord subject) => TicketGroupRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool CanAcceptProposal(this TicketGroupRecord subject, string principalId, ScopeContext context)
    {
        subject.NotNull();
        principalId.NotEmpty();

        bool access = subject switch
        {
            var v when v.Roles
                .Where(x => x.PrincipalId == principalId)
                .Any(x => x.MemberRole == RoleType.Owner || x.MemberRole == RoleType.Contributor) => true,

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

    public static bool IsOwner(this TicketGroupRecord ticketGroupRecord, string principalId)
    {
        ticketGroupRecord.Validate().ThrowOnError();
        principalId.NotEmpty();

        var state = ticketGroupRecord.Roles.Any(x => x.PrincipalId == principalId && x.MemberRole == RoleType.Owner);
        return state;
    }

    public static TicketGroupRecord SetOwner(this TicketGroupRecord ticketGroupRecord, string principalId)
    {
        ticketGroupRecord.Validate().ThrowOnError();
        principalId.NotEmpty();

        var roles = ticketGroupRecord.Roles
            .Where(x => x.PrincipalId != principalId)
            .Append(new RoleRecord { PrincipalId = principalId, MemberRole = RoleType.Owner })
            .ToArray();

        return ticketGroupRecord with { Roles = roles };
    }

    public static TicketGroupRecord SetTicketGroupId(this TicketGroupRecord subject, string principalId) => subject with
    {
        TicketGroupId = $"{principalId.NotEmpty()}/{subject.Name.NotEmpty()}",
    };

    public static TicketGroupRecord SetChannelId(this TicketGroupRecord subject) => subject with
    {
        ChannelId = subject.ChannelId.ToNullIfEmpty() ?? $"{subject.TicketGroupId.NotEmpty()}/channelHub",
    };
}
