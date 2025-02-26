namespace TicketMasterApi.sdk;

public record ClassificationRecord
{
    public string? SegmentId { get; init; }
    public string? Segment { get; init; }
    public string? GenreId { get; init; }
    public string? Genre { get; init; }
    public string? SubGenreId { get; init; }
    public string? SubGenre { get; init; }
}

