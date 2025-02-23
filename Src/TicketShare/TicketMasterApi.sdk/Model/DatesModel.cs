namespace TicketMasterApi.sdk;

public record DatesModel
{
    public StartModel Start { get; init; } = null!;
    public string? Timezone { get; init; }
}

public record StartModel
{
    public DateOnly? LocalDate { get; init; }
    public TimeOnly? LocalTime { get; init; }
    public string? TimeZone { get; init; }
}

