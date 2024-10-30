using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public enum ProposalState
{
    Open,
    Close,
}

public record ProposalRecord
{
    public string ProposalId { get; init; } = null!;
    public ProposalState State { get; init; }
    public string TicketGroupId { get; init; } = null!;
    public string AuthorPrincipalId { get; init; } = null!;
    public IReadOnlyList<string> Members { get; init; } = Array.Empty<string>();
    public string? Description { get; init; }
    public DateTime ProposedDate { get; init; }
    public DateTime? ClosedDate { get; init; }
    public DateTime? AppliedDate { get; init; }
    public string? AppliedByPrincipalId { get; init; }
    public IReadOnlyList<ProposalSeatRecord> Seats { get; init; } = Array.Empty<ProposalSeatRecord>();

    public static IValidator<ProposalRecord> Validator { get; } = new Validator<ProposalRecord>()
        .RuleFor(x => x.ProposalId).NotEmpty()
        .RuleFor(x => x.State).ValidEnum()
        .RuleFor(x => x.TicketGroupId).NotEmpty()
        .RuleFor(x => x.AuthorPrincipalId).NotEmpty()
        .RuleFor(x => x.Members).NotNull()
        .RuleFor(x => x.Members).Must(x => x.Count > 0 ? StatusCode.OK : (StatusCode.Conflict, "Must have a least 1 member"))
        .RuleFor(x => x.ProposedDate).ValidDateTime()
        .RuleFor(x => x.ClosedDate).ValidDateTimeOption()
        .RuleForEach(x => x.Seats).Validate(ProposalSeatRecord.Validator)
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
