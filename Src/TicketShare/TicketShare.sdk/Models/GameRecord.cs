namespace TicketShare.sdk;

[GenerateSerializer]
public record GameRecord
{
    [Id(0)] public string GameId { get; init; } = null!;
    [Id(1)] public string TeamId { get; init; } = null!;
    [Id(2)] public DateTime Date { get; init; }
    [Id(3)] public string Description { get; init; } = null!;
    [Id(4)] public string Season { get; init; } = null!;
}
