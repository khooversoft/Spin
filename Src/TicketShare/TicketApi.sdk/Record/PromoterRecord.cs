using TicketApi.sdk.Model;

namespace TicketApi.sdk;

public record PromoterRecord
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
}

public static class PromoterRecordExtensions
{
    public static PromoterRecord ConvertTo(this Event_PromoterModel subject) => new PromoterRecord
    {
        Id = subject.Id,
        Name = subject.Name,
        Description = subject.Description,
    };
}
