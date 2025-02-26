namespace TicketMasterApi.sdk;

public record LinkModel
{
    public HrefModel? First { get; init; }
    public HrefModel? Self { get; init; }
    public HrefModel? Next { get; init; }
    public HrefModel? Last { get; init; }
}
