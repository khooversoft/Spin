namespace TicketApi.sdk.Model;

public record Event_DatesModel
{
    public Event_StartModel Start { get; init; } = null!;
    public string? Timezone { get; init; }
}

public record Event_StartModel
{
    public DateOnly? LocalDate { get; init; }
    public TimeOnly? LocalTime { get; init; }
    public string? TimeZone { get; init; }
}

