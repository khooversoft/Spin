using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public enum CalendarRecordType
{
    Free,
    Busy
}

[GenerateSerializer]
[Alias("TicketShare.sdk.CalendarRecord")]
public record CalendarRecord
{
    [Id(0)] public CalendarRecordType Type { get; init; }
    [Id(1)] public DateTime FromDate { get; init; }
    [Id(2)] public DateTime ToDate { get; init; }

    public static IValidator<CalendarRecord> Validator { get; } = new Validator<CalendarRecord>()
        .RuleFor(x => x.Type).ValidEnum()
        .Build();
}

public static class CalendarRecordExtensions
{
    public static Option Validate(this CalendarRecord subject) => CalendarRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this CalendarRecord subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}