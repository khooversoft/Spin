namespace TicketShare.sdk;

public class EventRecordSelect
{
    public EventRecordSelect(string id, bool selected, DateTime date, string name)
    {
        Id = id;
        Selected = selected;
        Date = date;
        Name = name;
    }

    public string Id { get; }
    public bool Selected { get; set; }
    public DateTime Date { get; }
    public string Name { get; }
}
