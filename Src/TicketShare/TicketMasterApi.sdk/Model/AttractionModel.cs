namespace TicketMasterApi.sdk;

public record AttractionModel
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Url { get; init; }
    public IReadOnlyList<ImageModel> Images { get; init; } = Array.Empty<ImageModel>();
}
