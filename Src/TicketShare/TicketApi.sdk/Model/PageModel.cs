namespace TicketApi.sdk.Model;

public record PageModel
{
    public int? Size { get; init; }
    public int? TotalElements { get; init; }
    public int? TotalPages { get; init; }
    public int? Number { get; init; }
}
