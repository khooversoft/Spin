using Toolbox.Extensions;
using Toolbox.Tools;

namespace TicketShare.sdk;

public sealed record ProposeSeatChange
{
    public string SeatId { get; init; } = null!;
    public DateTime Date { get; init; } = DateTime.Now;
    public string AssignedToPrincipalId { get; init; } = null!;
    public SeatChangeConfirmed? Confirm { get; init; }

    public bool Equals(ProposeSeatChange? other) =>
        other != null &&
        SeatId.EqualsIgnoreCase(other.SeatId) &&
        Date == other.Date &&
        AssignedToPrincipalId.EqualsIgnoreCase(other.AssignedToPrincipalId) &&
        Confirm == other.Confirm;

    public override int GetHashCode() => HashCode.Combine(SeatId, Date, AssignedToPrincipalId);

    public static IValidator<ProposeSeatChange> Validator { get; } = new Validator<ProposeSeatChange>()
        .RuleFor(x => x.SeatId).NotEmpty()
        .RuleFor(x => x.Date).ValidDateTime()
        .RuleFor(x => x.AssignedToPrincipalId).NotEmpty()
        .RuleFor(x => x.Confirm).ValidateOption(SeatChangeConfirmed.Validator)
        .Build();
}
