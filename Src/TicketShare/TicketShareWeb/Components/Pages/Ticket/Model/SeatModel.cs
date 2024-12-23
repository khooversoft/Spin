using TicketShare.sdk;

namespace TicketShareWeb.Components.Pages.Ticket.Model;

public record SeatModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SeatId { get; set; } = null!;
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public string? AssignedToPrincipalId { get; set; }

    public string GetKey() => $"{SeatId}-{Date.ToString("yyyyMMdd")}";
}


public static class SeatModelExtensions
{
    public static string GetKey(this SeatRecord subject) => $"{subject.SeatId}-{subject.Date.ToString("yyyyMMdd")}";

    public static SeatModel Clone(this SeatModel subject) => new SeatModel
    {
        Id = subject.Id,
        SeatId = subject.SeatId,
        Date = subject.Date,
        AssignedToPrincipalId = subject.AssignedToPrincipalId,
    };

    public static SeatModel ConvertTo(this SeatRecord subject) => new SeatModel
    {
        Id = subject.Id,
        SeatId = subject.SeatId,
        Date = subject.Date,
        AssignedToPrincipalId = subject.AssignedToPrincipalId,
    };

    public static SeatRecord ConvertTo(this SeatModel subject) => new SeatRecord
    {
        Id = subject.Id,
        SeatId = subject.SeatId,
        Date = subject.Date,
        AssignedToPrincipalId = subject.AssignedToPrincipalId,
    };
}
