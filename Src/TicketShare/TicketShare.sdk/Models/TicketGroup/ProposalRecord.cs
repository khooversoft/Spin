using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;


/// <summary>
/// Lifecycle
///   1) Create proposal with members, only members can make changes.  Author can add or remove members.
///   2) User either accept or reject each or all seats in the proposal(s)
///   3) Applied or Closed, the proposal cannot be changed.  New proposals must be created.
/// </summary>
public sealed record ProposalRecord
{
    public string ProposalId { get; init; } = Guid.NewGuid().ToString();
    public string SeatId { get; init; } = null!;
    public StateDetail Proposed { get; init; } = null!;
    public StateDetail? Applied { get; init; }
    public StateDetail? Rejected { get; init; }
    public StateDetail? Closed { get; init; }
    public ProposalResponse? Resolution { get; init; }

    public bool Equals(ProposalRecord? obj)
    {
        var state = obj is ProposalRecord subject &&
            ProposalId == subject.ProposalId &&
            Proposed == subject.Proposed &&
            Applied == subject.Applied &&
            Rejected == subject.Rejected &&
            Closed == subject.Closed &&
            Resolution == subject.Resolution;

        return true;
    }

    public override int GetHashCode() => HashCode.Combine(ProposalId, SeatId, Proposed, Applied, Rejected, Closed, Resolution);

    public static IValidator<ProposalRecord> Validator { get; } = new Validator<ProposalRecord>()
        .RuleFor(x => x.ProposalId).NotEmpty()
        .RuleFor(x => x.SeatId).NotEmpty()
        .RuleFor(x => x.Proposed).NotNull()
        .Build();
}

public record StateDetail
{
    public DateTime Date { get; init; }
    public string ByPrincipalId { get; init; } = null!;

    public static IValidator<StateDetail> Validator { get; } = new Validator<StateDetail>()
        .RuleFor(x => x.ByPrincipalId).NotEmpty()
        .RuleFor(x => x.Date).ValidDateTime()
        .Build();
}


public static class ProposalRecordTool
{
    public static Option Validate(this ProposalRecord subject) => ProposalRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ProposalRecord subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
