namespace TicketMasterApi.sdk.Model.Event;

public record Event_AttractionModel
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Url { get; init; }
    public IReadOnlyList<Event_ImageModel> Images { get; init; } = Array.Empty<Event_ImageModel>();
}
