namespace TicketMasterApi.sdk.MasterList;

public record TeamDetail
{
    public string League { get; init; } = null!;
    public string Name { get; init; } = null!;
    public IReadOnlyList<TeamClassification> Segments { get; init; } = Array.Empty<TeamClassification>();
    public IReadOnlyList<TeamClassification> Genres { get; init; } = Array.Empty<TeamClassification>();
    public IReadOnlyList<TeamClassification> SubGenres { get; init; } = Array.Empty<TeamClassification>();
}

public record TeamClassification
{
    public string Attribute { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Id { get; init; } = null!;
}