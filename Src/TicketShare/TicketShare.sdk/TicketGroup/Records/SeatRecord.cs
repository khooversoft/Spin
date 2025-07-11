using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public sealed record SeatRecord : IEquatable<SeatRecord>
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Section { get; set; } = null!;
    public string Row { get; set; } = null!;
    public string Seat { get; set; } = null!;

    public DateTime Date { get; init; }
    public string? AssignedToPrincipalId { get; init; }

    public override string ToString() => $"{Section}-{Row}-{Seat}";

    public bool Equals(SeatRecord? obj) =>
        obj is SeatRecord subject &&
        Section == subject.Section &&
        Row == subject.Row &&
        Seat == subject.Seat &&
        Date == subject.Date &&
        AssignedToPrincipalId == subject.AssignedToPrincipalId == true;

    public override int GetHashCode() => HashCode.Combine(Id, Section, Row, Seat, Date, AssignedToPrincipalId);

    public static IValidator<SeatRecord> Validator { get; } = new Validator<SeatRecord>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.Section).NotEmpty()
        .RuleFor(x => x.Row).NotEmpty()
        .RuleFor(x => x.Seat).NotEmpty()
        .RuleFor(x => x.Date).ValidDateTime()
        .Build();
}


public static class SeatRecordExtensions
{
    public static Option Validate(this SeatRecord subject) => SeatRecord.Validator.Validate(subject).ToOptionStatus();
}