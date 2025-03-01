namespace TicketMasterApi.sdk;


public record EventRecord
{
    public string Id { get; init; } = null!;
    public DateTime? LocalDate { get; init; }
    public string? Timezone { get; init; }
    public IReadOnlyList<PromoterRecord> Promoters { get; init; } = Array.Empty<PromoterRecord>();
    public string? SeatMapUrl { get; init; }
    public ClassificationRecord? Classification { get; init; }
    public IReadOnlyList<VenueRecord> Venues { get; init; } = Array.Empty<VenueRecord>();
    public IReadOnlyList<AttractionRecord> Attractions { get; init; } = Array.Empty<AttractionRecord>();
}

