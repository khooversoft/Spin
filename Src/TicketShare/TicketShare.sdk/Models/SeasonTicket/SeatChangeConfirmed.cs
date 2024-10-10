using Toolbox.Tools;

namespace TicketShare.sdk;

public sealed record SeatChangeConfirmed
{
    public DateTime Date { get; init; }
    public bool Confirm { get; init; }
    public string ByPrincipalId { get; init; } = null!;

    public bool Equals(SeatChangeConfirmed? other) =>
        other != null &&
        Date == other.Date &&
        Confirm == other.Confirm &&
        ByPrincipalId.Equals(other.ByPrincipalId);

    public override int GetHashCode() => HashCode.Combine(Date, Confirm, ByPrincipalId);

    public static IValidator<SeatChangeConfirmed> Validator { get; } = new Validator<SeatChangeConfirmed>()
        .RuleFor(x => x.Date).ValidDateTime()
        .RuleFor(x => x.ByPrincipalId).NotEmpty()
        .Build();
}
