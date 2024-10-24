namespace TicketShare.sdk;

public record TeamRecord
{
    public string TeamId { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
    public string Sport { get; init; } = null!;
    public string League { get; init; } = null!;
}
