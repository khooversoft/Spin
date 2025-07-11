using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public enum CalendarRecordType
{
    Busy,
    Tenative
}

public record CalendarRecord
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public CalendarRecordType Type { get; init; }
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }

    public static IValidator<CalendarRecord> Validator { get; } = new Validator<CalendarRecord>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.Type).ValidEnum()
        .RuleFor(x => x.FromDate).ValidDateTime()
        .RuleFor(x => x.ToDate).ValidDateTime()
        .Build();
}

public static class CalendarRecordExtensions
{
    public static Option Validate(this CalendarRecord subject) => CalendarRecord.Validator.Validate(subject).ToOptionStatus();
}