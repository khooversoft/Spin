namespace TicketMasterApi.sdk.Model.Event;

public record Event_VenueModel
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? PostalCode { get; init; }
    public string? TimeZone { get; init; }
    public Event_StateModel? State { get; init; }
    public Event_CityModel? City { get; init; }
    public Event_CountryModel? Country { get; init; }
    public Event_AddressModel? Address { get; init; }
}

public record Event_CityModel
{
    public string? Name { get; init; }
}

public record Event_StateModel
{
    public string? Name { get; init; }
    public string? StateCode { get; init; }
}

public record Event_CountryModel
{
    public string? Name { get; init; }
    public string? CountryCode { get; init; }
}

public record Event_AddressModel
{
    public string? Line1 { get; init; }
}
