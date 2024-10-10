namespace TicketShare.sdk;

[GenerateSerializer]
public record TeamRecord
{
    [Id(0)] public string TeamId { get; init; } = null!;
    [Id(1)] public string Name { get; init; } = null!;
    [Id(2)] public string Description { get; init; } = null!;
    [Id(3)] public string Sport { get; init; } = null!;
    [Id(4)] public string League { get; init; } = null!;
}
