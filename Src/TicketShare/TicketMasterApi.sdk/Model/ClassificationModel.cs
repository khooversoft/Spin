namespace TicketMasterApi.sdk;

public record ClassificationModel
{
    public ClassificationSubModel? Segment { get; init; }
    public ClassificationSubModel? Genre { get; init; }
    public ClassificationSubModel? SubGenre { get; init; }
}

public record ClassificationSubModel
{
    public string Id { get; init; } = null!;
    public string? Name { get; init; }
}
