namespace TicketShare.sdk;

public record GameRecord
{
    public string GameId { get; init; } = null!;
    public string TeamId { get; init; } = null!;
    public DateTime Date { get; init; }
    public string Description { get; init; } = null!;
    public string Season { get; init; } = null!;
}
