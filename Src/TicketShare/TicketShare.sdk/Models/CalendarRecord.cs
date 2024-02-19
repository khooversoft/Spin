namespace TicketShare.sdk;

public enum CalendarRecordType
{
    None,
    Free,
    Busy
}

public record CalendarRecord
{
    public CalendarRecordType Type { get; init; }
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
}
