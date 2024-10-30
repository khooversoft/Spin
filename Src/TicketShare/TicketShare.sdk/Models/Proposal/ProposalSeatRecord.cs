using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public record ProposalSeatRecord
{
    public string TicketGroupId { get; init; } = null!;
    public string SeatId { get; init; } = null!;
    public DateTime Date { get; init; }
    public string? CurrentlyAssignedToPrincipalId { get; init; }
    public string? ProposedAssignedToPrincipalId { get; init; }
    public ProposalResponse? Resolution { get; init; }

    public static IValidator<ProposalSeatRecord> Validator { get; } = new Validator<ProposalSeatRecord>()
        .RuleFor(x => x.TicketGroupId).NotEmpty()
        .RuleFor(x => x.SeatId).NotEmpty()
        .RuleFor(x => x.Date).ValidDateTime()
        .RuleFor(x => x.Resolution).ValidateOption(ProposalResponse.Validator)
        .Build();
}

public static class ProposalSeatRecordTool
{
    public static Option Validate(this ProposalSeatRecord subject) => ProposalSeatRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ProposalSeatRecord subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
