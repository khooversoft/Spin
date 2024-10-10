using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

[GenerateSerializer]
[Alias("TicketShare.sdk.SeatRecord")]
public sealed record SeatRecord : IEquatable<SeatRecord>
{
    [Id(0)] public string SeatId { get; init; } = null!;
    [Id(1)] public DateTime Date { get; init; } = DateTime.Now;
    [Id(2)] public string? AssignedToPrincipalId { get; init; }

    public bool Equals(SeatRecord? other) =>
        other != null &&
        SeatId.Equals(other.SeatId) &&
        Date == other.Date &&
        AssignedToPrincipalId?.Equals(other.AssignedToPrincipalId) == true;

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