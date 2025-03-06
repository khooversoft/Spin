namespace TicketShare.sdk;

public record SeatModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Section { get; set; } = null!;
    public string Row { get; set; } = null!;
    public string Seat { get; set; } = null!;
    public DateTime? Date { get; set; } = DateTime.Now.Date;
    public string? AssignedToPrincipalId { get; set; }

    public override string ToString() => $"{Section}-{Row}-{Seat}";
}


public static class SeatModelExtensions
{
    public static SeatModel Clone(this SeatModel subject) => new SeatModel
    {
        Id = subject.Id,
        Section = subject.Section,
        Row = subject.Row,
        Seat = subject.Seat,
        Date = subject.Date,
        AssignedToPrincipalId = subject.AssignedToPrincipalId,
    };

    public static SeatModel ConvertTo(this SeatRecord subject) => new SeatModel
    {
        Id = subject.Id,
        Row = subject.Row,
        Seat = subject.Seat,
        Date = subject.Date,
        AssignedToPrincipalId = subject.AssignedToPrincipalId,
    };

    public static SeatRecord ConvertTo(this SeatModel subject) => new SeatRecord
    {
        Id = subject.Id,
        Row = subject.Row,
        Seat = subject.Seat,
        Date = subject.Date ?? DateTime.UtcNow,
        AssignedToPrincipalId = subject.AssignedToPrincipalId,
    };
}
