namespace TicketMasterApi.sdk.Model.Event;

public record Event_EventRecordModel
{
    public string Id { get; init; } = null!;
    public string? Name { get; init; }
    public IReadOnlyList<Event_ImageModel> Images { get; init; } = Array.Empty<Event_ImageModel>();
    public Event_DatesModel? Dates { get; init; }
    public IReadOnlyList<Event_ClassificationModel> Classifications { get; init; } = Array.Empty<Event_ClassificationModel>();
    public PromoterRecord? Promoter { get; init; }
    public IReadOnlyList<Event_PromoterModel> Promoters { get; init; } = Array.Empty<Event_PromoterModel>();
    public Event_Seatmap? Seatmap { get; init; }
    public Event_EventEmbedded? _embedded { get; init; }
}

public record Event_Seatmap
{
    public string Id { get; init; } = null!;
    public string? StaticUrl { get; init; }
}

public record Event_PromoterModel
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
}

public record Event_EventEmbedded
{
    public IReadOnlyList<Event_VenueModel> Venues { get; init; } = Array.Empty<Event_VenueModel>();
    public IReadOnlyList<Event_AttractionModel> Attractions { get; init; } = Array.Empty<Event_AttractionModel>();
}

