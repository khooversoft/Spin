namespace TicketShare.sdk;

public enum TicketType
{
    None,
    Package,
    Season,
    Seat
}

public record TicketRecord
{
    public string TicketId { get; init; } = null!;
    public string GameId { get; init; } = null!;
    public TicketType Type { get; init; }
    public DateTime Date { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string? SeatNumber { get; init; }
    public string? Season { get; init; }
    public TicketRelationship ToParent { get; init; } = null!;
}


public enum TicketRelationshipType
{
    None,
    SeasonToSeat,
    PackageToSeason,
}

public record TicketRelationship
{
    public string ParentTicketId { get; init; } = null!;
    public TicketRelationshipType Type { get; init; }
}
