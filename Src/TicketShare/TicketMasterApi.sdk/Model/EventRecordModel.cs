namespace TicketMasterApi.sdk;

public record EventRecordModel
{
    public string Id { get; init; } = null!;
    public string? Name { get; init; }
    public IReadOnlyList<ImageModel> Images { get; init; } = Array.Empty<ImageModel>();
    public DatesModel? Dates { get; init; }
    public IReadOnlyList<ClassificationModel> Classifications { get; init; } = Array.Empty<ClassificationModel>();
    public PromoterRecord? Promoter { get; init; }
    public IReadOnlyList<PromoterModel> Promoters { get; init; } = Array.Empty<PromoterModel>();
    public Seatmap? Seatmap { get; init; }
    public EventEmbedded? _embedded { get; init; }
}

public record Seatmap
{
    public string Id { get; init; } = null!;
    public string? StaticUrl { get; init; }
}

public record PromoterModel
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
}

public record EventEmbedded
{
    public IReadOnlyList<VenueModel> Venues { get; init; } = Array.Empty<VenueModel>();
    public IReadOnlyList<AttractionModel> Attractions { get; init; } = Array.Empty<AttractionModel>();
}

