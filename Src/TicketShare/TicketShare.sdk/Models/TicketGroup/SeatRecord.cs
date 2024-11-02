using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public sealed record SeatRecord : IEquatable<SeatRecord>
{
    public string SeatId { get; init; } = null!;
    public DateTime Date { get; init; }
    public string? AssignedToPrincipalId { get; init; }

    public bool Equals(SeatRecord? obj) =>
        obj is SeatRecord subject &&
        SeatId == subject.SeatId &&
        Date == subject.Date &&
        AssignedToPrincipalId == subject.AssignedToPrincipalId == true;

    public override int GetHashCode() => HashCode.Combine(SeatId, Date, AssignedToPrincipalId);

    public static IValidator<SeatRecord> Validator { get; } = new Validator<SeatRecord>()
        .RuleFor(x => x.SeatId).NotEmpty()
        .RuleFor(x => x.Date).ValidDateTime()
        .Build();
}


public static class SeatRecordExtensions
{
    public static Option Validate(this SeatRecord subject) => SeatRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this SeatRecord subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}