using System.ComponentModel.DataAnnotations;
using TicketShare.sdk;

namespace TicketShareWeb.Components.Pages.Profile.Models;

public sealed record CalendarModel
{
    public string Id { get; init; } = null!;

    public CalendarRecordType Type { get; set; }
    [Display(Name = "From Date")]
    public DateTime FromDate { get; set; }

    [Display(Name = "To Date")]
    public DateTime ToDate { get; set; }
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
        Type = subject.Type,
        FromDate = subject.FromDate,
        ToDate = subject.ToDate,
    };

    public static CalendarRecord ConvertTo(this CalendarModel subject) => new CalendarRecord
    {
        Id = subject.Id,
        Type = subject.Type,
        FromDate = subject.FromDate,
        ToDate = subject.ToDate,
    };
}