using TicketShare.sdk;

namespace TicketShareWeb.Components.Pages.Profile.Models;

public sealed record CalendarModel
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = null!;
    public DateTime FromDate { get; set; } = DateTime.Now.Date;
    public DateTime ToDate { get; set; } = DateTime.Now.Date;
}


public static class CalendarModelExtensions
{
    public static CalendarModel Clone(this CalendarModel subject) => new CalendarModel
    {
        Id = subject.Id,
        Type = subject.Type,
        FromDate = subject.FromDate,
        ToDate = subject.ToDate,
    };

    public static CalendarModel ConvertTo(this CalendarRecord subject) => new CalendarModel
    {
        Id = subject.Id,
        Type = subject.Type.ToString(),
        FromDate = subject.FromDate,
        ToDate = subject.ToDate,
    };

    public static CalendarRecord ConvertTo(this CalendarModel subject) => new CalendarRecord
    {
        Id = subject.Id,
        Type = Enum.Parse<CalendarRecordType>(subject.Type),
        FromDate = subject.FromDate,
        ToDate = subject.ToDate,
    };
}