using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public record SeatRecord
{
    public string SeatId { get; init; } = null!;
    public DateTime Date { get; init; }
    public string? AssignedToPrincipalId { get; init; }

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
