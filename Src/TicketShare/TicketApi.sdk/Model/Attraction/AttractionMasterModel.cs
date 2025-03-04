namespace TicketApi.sdk.Model;

public record AttractionMasterModel
{
    public Attraction_Embedded? _embedded { get; init; }
    public LinkModel? _links { get; init; }
    public PageModel? Page { get; init; }
}

public record Attraction_Embedded
{
    public IReadOnlyList<AttractionModel> Attractions { get; init; } = Array.Empty<AttractionModel>();
}
